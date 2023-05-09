using System;
using System.Net;
using System.Threading.Tasks;
using Unity.Services.Authentication.Shared;
using UnityEngine.Networking;

namespace Unity.Services.Authentication
{
    static class AuthenticationWebRequestUtils
    {
        public static Task<ApiResponse> SendWebRequestAsync(this UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<ApiResponse>();
            var asyncOp = request.SendWebRequest();

            if (asyncOp.isDone)
            {
                ProcessResponse(tcs, request);
            }
            else
            {
                asyncOp.completed += asyncOperation =>
                {
                    ProcessResponse(tcs, request);
                };
            }

            return tcs.Task;
        }

        public static Task<ApiResponse<T>> SendWebRequestAsync<T>(this UnityWebRequest request, System.Threading.CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<ApiResponse<T>>();
            cancellationToken.Register(() => tcs.SetCanceled());

            var asyncOp = request.SendWebRequest();

            if (asyncOp.isDone)
            {
                ProcessResponse(tcs, request);
            }
            else
            {
                asyncOp.completed += asyncOperation =>
                {
                    ProcessResponse(tcs, request);
                };
            }

            return tcs.Task;
        }

        static void ProcessResponse(TaskCompletionSource<ApiResponse> tcs, UnityWebRequest request)
        {
            var response = new ApiResponse()
            {
                StatusCode = (int)request.responseCode,
                ErrorText = request.error,
                RawContent = request.downloadHandler?.text,
            };

            var error = $"{request.error}\n{request.downloadHandler?.text}";

            if (IsNetworkError(request))
            {
                tcs.SetException(new ApiException(ApiExceptionType.Network, error, response));
            }
            else if (IsHttpError(request))
            {
                tcs.SetException(new ApiException(ApiExceptionType.Http, error, response));
            }
            else
            {
                tcs.SetResult(response);
            }
        }

        static void ProcessResponse<T>(TaskCompletionSource<ApiResponse<T>> tcs, UnityWebRequest request)
        {
            var response = new ApiResponse<T>()
            {
                StatusCode = (int)request.responseCode,
                ErrorText = request.error,
                RawContent = request.downloadHandler?.text,
            };

            var error = $"{request.error}\n{request.downloadHandler?.text}";

            if (IsNetworkError(request))
            {
                tcs.SetException(new ApiException(ApiExceptionType.Network, error, response));
            }
            else if (IsHttpError(request))
            {
                tcs.SetException(new ApiException(ApiExceptionType.Http, error, response));
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(request.downloadHandler?.text))
                    {
                        response.Data = IsolatedJsonConvert.DeserializeObject<T>(request.downloadHandler?.text, SerializerSettings.DefaultSerializerSettings);
                    }
                }
                catch (Exception)
                {
                    tcs.SetException(new ApiException(ApiExceptionType.Deserialization, $"Deserialization of type '{typeof(T)}' failed.", response));
                    return;
                }

                tcs.SetResult(response);
            }
        }

        public static bool IsNetworkError(UnityWebRequest request)
        {
            return request.responseCode >= 500;
        }

        public static bool IsHttpError(UnityWebRequest request)
        {
            return request.responseCode >= 400 && request.responseCode < 500;
        }
    }
}
