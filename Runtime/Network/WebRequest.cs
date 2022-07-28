using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Unity.Services.Authentication
{
    class WebRequest
    {
        readonly WebRequestVerb m_Verb;
        readonly string m_Url;
        readonly IDictionary<string, string> m_Headers;
        readonly string m_Payload;
        readonly string m_PayloadContentType;
        readonly JsonSerializerSettings m_JsonSerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        internal INetworkConfiguration Configuration { get; }
        internal int Retries { get; private set; }

        internal WebRequest(INetworkConfiguration configuration,
                            WebRequestVerb verb,
                            string url,
                            IDictionary<string, string> headers,
                            string payload,
                            string payloadContentType)
        {
            Configuration = configuration;
            m_Verb = verb;
            m_Url = url;
            m_Headers = headers;
            m_Payload = payload;
            m_PayloadContentType = payloadContentType;
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
                return default;
            }
            else
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(textResult, m_JsonSerializerSettings);
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
#if UNITY_2022_2_OR_NEWER
                        unityWebRequest = UnityWebRequest.PostWwwForm(m_Url, string.Empty);
#else
                        unityWebRequest = UnityWebRequest.Post(m_Url, string.Empty);
#endif
                    }
                    else
                    {
                        unityWebRequest = new UnityWebRequest(m_Url, UnityWebRequest.kHttpVerbPOST)
                        {
                            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(m_Payload)),
                            downloadHandler = new DownloadHandlerBuffer()
                        };
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
                        unityWebRequest = new UnityWebRequest(m_Url, UnityWebRequest.kHttpVerbPUT)
                        {
                            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(m_Payload)),
                            downloadHandler = new DownloadHandlerBuffer()
                        };
                    }
                    break;
                case WebRequestVerb.Delete:
                    unityWebRequest = UnityWebRequest.Delete(m_Url);
                    unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
                    break;
                default:
                    throw new ArgumentException("Unknown verb " + m_Verb);
            }

            if (!string.IsNullOrEmpty(m_PayloadContentType))
            {
                unityWebRequest.SetRequestHeader("Content-Type", m_PayloadContentType);
            }

            if (m_Headers != null)
            {
                foreach (var headerAndValue in m_Headers)
                {
                    unityWebRequest.SetRequestHeader(headerAndValue.Key, headerAndValue.Value);
                }
            }

            unityWebRequest.timeout = Configuration.Timeout;
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

            if (isNetworkError && Retries < Configuration.Retries)
            {
                Logger.LogWarning("Network error detected, retrying...");
                Retries++;
                SendAttemptAsync(tcs);
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
