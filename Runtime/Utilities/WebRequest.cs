using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Core.Scheduler.Internal;
using UnityEngine.Networking;

namespace Unity.Services.Authentication.Utilities
{
    enum WebRequestVerb
    {
        Get,
        Post,
        Put,
        Delete
    }

    class WebRequest
    {
        const int k_DefaultTimeoutSeconds = 10;
        const int k_RetryBackoffSeconds = 10;

        readonly IActionScheduler m_Scheduler;
        readonly WebRequestVerb m_Verb;
        readonly string m_Url;
        readonly IDictionary<string, string> m_Headers;
        readonly string m_Payload;
        readonly string m_PayloadContentType;
        readonly int m_RedirectLimit;
        int m_Attempts;

        internal int RequestTimeout { get; set; }
        internal int Attempts => m_Attempts;

        internal WebRequest(IActionScheduler scheduler,
                            WebRequestVerb verb,
                            string url,
                            IDictionary<string, string> headers,
                            string payload,
                            string payloadContentType,
                            int redirectLimit,
                            int attempts)
        {
            m_Scheduler = scheduler;
            m_Verb = verb;
            m_Url = url;
            m_Headers = headers;
            m_Payload = payload;
            m_PayloadContentType = payloadContentType;
            m_RedirectLimit = redirectLimit;
            m_Attempts = attempts;

            RequestTimeout = k_DefaultTimeoutSeconds;
        }

        internal Task SendAsync()
        {
            return SendAttemptAsync(new TaskCompletionSource<string>());
        }

        internal async Task<T> SendAsync<T>()
        {
            var textResult = await SendAttemptAsync(new TaskCompletionSource<string>());
            Logger.LogVerbose("Request completed successfully!");

            // Check if the response body has any contents to parse
            if (string.IsNullOrEmpty(textResult))
            {
                return default(T);
            }
            else
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(textResult);
                }
                catch (Exception e)
                {
                    var errorMessage = "Failed to deserialize object!";
                    Logger.LogError($"{errorMessage} {e.Message}");
                    throw new WebRequestException(false, false, true, 0, errorMessage, null);
                }
            }
        }

        Task<string> SendAttemptAsync(TaskCompletionSource<string> tcs)
        {
            try
            {
                var request = Build();
                request.SendWebRequest().completed += (operation) =>
                {
                    RequestCompleted(tcs,
                        request.responseCode,
                        RequestHasNetworkError(request),
                        RequestHasServerError(request),
                        request.error,
                        request.downloadHandler?.text,
                        request.GetResponseHeaders());
                    request.Dispose();
                    m_Attempts--;
                };
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }

            return tcs.Task;
        }

        internal UnityWebRequest Build()
        {
            Logger.LogVerbose($"[WebRequest] {m_Verb.ToString().ToUpper()} {m_Url}\n" +
                $"{string.Join("\n", m_Headers?.Select(x => x.Key + ": " + x.Value) ?? new string[] { })}\n" +
                (m_Payload ?? ""));

            UnityWebRequest unityWebRequest;

            switch (m_Verb)
            {
                case WebRequestVerb.Post:
                    if (string.IsNullOrEmpty(m_Payload))
                    {
                        unityWebRequest = UnityWebRequest.Post(m_Url, string.Empty);
                    }
                    else
                    {
                        var postBytes = Encoding.UTF8.GetBytes(m_Payload);
                        unityWebRequest = new UnityWebRequest(m_Url, UnityWebRequest.kHttpVerbPOST);
                        unityWebRequest.uploadHandler = new UploadHandlerRaw(postBytes)
                        {
                            contentType = m_PayloadContentType
                        };
                        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                    }

                    break;
                case WebRequestVerb.Get:
                    unityWebRequest = UnityWebRequest.Get(m_Url);
                    break;
                case WebRequestVerb.Put:
                    if (string.IsNullOrEmpty(m_Payload))
                    {
                        // UnityWebRequest doesn't allow empty put request body.
                        throw new ArgumentException("PUT payload cannot be empty.");
                    }
                    else
                    {
                        var putBytes = Encoding.UTF8.GetBytes(m_Payload);
                        unityWebRequest = new UnityWebRequest(m_Url, UnityWebRequest.kHttpVerbPUT);
                        unityWebRequest.uploadHandler = new UploadHandlerRaw(putBytes)
                        {
                            contentType = m_PayloadContentType
                        };
                        unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                    }
                    break;
                case WebRequestVerb.Delete:
                    unityWebRequest = UnityWebRequest.Delete(m_Url);
                    unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                    break;
                default:
                    throw new ArgumentException("Unknown verb " + m_Verb);
            }

            if (m_Headers != null)
            {
                foreach (var headerAndValue in m_Headers)
                {
                    unityWebRequest.SetRequestHeader(headerAndValue.Key, headerAndValue.Value);
                }
            }

            unityWebRequest.redirectLimit = m_RedirectLimit;
            unityWebRequest.timeout = RequestTimeout;
            return unityWebRequest;
        }

        internal void RequestCompleted(
            TaskCompletionSource<string> tcs,
            long responseCode,
            bool isNetworkError,
            bool isServerError,
            string errorText,
            string bodyText,
            IDictionary<string, string> headers)
        {
            Logger.LogVerbose($"[WebResponse] {m_Verb.ToString().ToUpper()} {m_Url}\n" +
                $"{string.Join("\n", headers?.Select(x => x.Key + ": " + x.Value) ?? new string[] { })}\n" +
                $"{bodyText}\n{errorText}\n");

            if (isNetworkError && m_Scheduler != null && m_Attempts > 0)
            {
                Logger.LogWarning("Network error detected, retrying...");
                m_Scheduler.ScheduleAction(() => SendAttemptAsync(tcs), k_RetryBackoffSeconds);
            }
            else
            {
                if (isNetworkError || isServerError)
                {
                    // If this is a service error rather than a network error, return the response body
                    // as that is likely to contain a service-appropriate error message in this case.
                    var errorMessage = (isServerError && !string.IsNullOrEmpty(bodyText)) ? bodyText : errorText;
                    var exception = new WebRequestException(isNetworkError, isServerError, false, responseCode, errorMessage, headers);
                    tcs.SetException(exception);
                    Logger.LogWarning($"Request completed with error: {errorMessage}");
                }
                else
                {
                    tcs.SetResult(bodyText);
                }
            }
        }

        bool RequestHasServerError(UnityWebRequest request)
        {
            return request.responseCode >= 400;
        }

        bool RequestHasNetworkError(UnityWebRequest request)
        {
#if UNITY_2020_2_OR_NEWER
            return request.result == UnityWebRequest.Result.ConnectionError && request.error != "Redirect limit exceeded";
#else
            return request.isNetworkError && request.error != "Redirect limit exceeded";
#endif
        }
    }
}
