using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication.Server.Shared;
using UnityEngine.Networking;

namespace Unity.Services.Authentication.Server
{
    static class WebRequestUtils
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

            if (IsNetworkError(request))
            {
                tcs.SetException(new ApiException(ApiExceptionType.Network, request.error, response));
            }
            else if (IsHttpError(request))
            {
                tcs.SetException(new ApiException(ApiExceptionType.Http, request.error, response));
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

            if (IsNetworkError(request))
            {
                tcs.SetException(new ApiException(ApiExceptionType.Network, request.error, response));
            }
            else if (IsHttpError(request))
            {
                tcs.SetException(new ApiException(ApiExceptionType.Http, request.error, response));
            }
            else
            {
                try
                {
                    if (!string.IsNullOrEmpty(request.downloadHandler?.text))
                    {
                        response.Data = JsonConvert.DeserializeObject<T>(request.downloadHandler?.text);
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

        static bool IsNetworkError(UnityWebRequest request)
        {
#if UNITY_2020_1_OR_NEWER
            return request.result == UnityWebRequest.Result.ConnectionError;
#else
            return request.isNetworkError;
#endif
        }

        static bool IsHttpError(UnityWebRequest request)
        {
#if UNITY_2020_1_OR_NEWER
            return request.result == UnityWebRequest.Result.ProtocolError;
#else
            return request.isHttpError;
#endif
        }
    }
}
