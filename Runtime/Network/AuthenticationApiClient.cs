using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication.Shared;
using UnityEngine.Networking;

namespace Unity.Services.Authentication
{
    class AuthenticationApiClient : IApiClient
    {
        INetworkConfiguration Configuration { get; }

        public AuthenticationApiClient(INetworkConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Task<ApiResponse> GetAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, WebRequestVerb.Get, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> GetAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, WebRequestVerb.Get, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> PostAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, WebRequestVerb.Post, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> PostAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, WebRequestVerb.Post, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> PutAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, WebRequestVerb.Put, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> PutAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, WebRequestVerb.Put, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> DeleteAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, WebRequestVerb.Delete, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> DeleteAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, WebRequestVerb.Delete, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> HeadAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, WebRequestVerb.Head, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> HeadAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, WebRequestVerb.Head, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> OptionsAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, WebRequestVerb.Options, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> OptionsAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, WebRequestVerb.Options, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> PatchAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, WebRequestVerb.Patch, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> PatchAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, WebRequestVerb.Patch, options, configuration, cancellationToken);
        }

        async Task<ApiResponse> SendAsync(string path, WebRequestVerb method, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            bool shouldRetry;
            var attempts = 0;

            do
            {
                try
                {
                    using (var request = BuildWebRequest(path, method, options, configuration))
                    {
                        return await AuthenticationWebRequestUtils.SendWebRequestAsync(request);
                    }
                }
                catch (ApiException e)
                {
                    shouldRetry = e.Type == ApiExceptionType.Network;
                    attempts++;

                    if (attempts >= Configuration.Retries || !shouldRetry)
                    {
                        throw;
                    }
                }
            }
            while (shouldRetry);

            return null;
        }

        async Task<ApiResponse<T>> SendAsync<T>(string path, WebRequestVerb method, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            bool shouldRetry;
            var attempts = 0;

            do
            {
                try
                {
                    using (var request = BuildWebRequest(path, method, options, configuration))
                    {
                        return await AuthenticationWebRequestUtils.SendWebRequestAsync<T>(request, cancellationToken);
                    }
                }
                catch (ApiException e)
                {
                    shouldRetry = e.Type == ApiExceptionType.Network;

                    if (attempts >= Configuration.Retries || !shouldRetry)
                    {
                        throw;
                    }

                    attempts++;
                }
            }
            while (shouldRetry);

            return null;
        }

        internal UnityWebRequest BuildWebRequest(string path, WebRequestVerb method, ApiRequestOptions options, IApiConfiguration configuration)
        {
            var builder = new ApiRequestPathBuilder(configuration.BasePath, path);
            builder.AddPathParameters(options.PathParameters);
            builder.AddQueryParameters(options.QueryParameters);
            var uri = builder.GetFullUri();

            var request = new UnityWebRequest(uri, method.ToString());

            if (configuration.UserAgent != null)
            {
                request.SetRequestHeader("User-Agent", configuration.UserAgent);
            }

            if (configuration.DefaultHeaders != null)
            {
                foreach (var headerParam in configuration.DefaultHeaders)
                {
                    request.SetRequestHeader(headerParam.Key, headerParam.Value);
                }
            }

            if (options.HeaderParameters != null)
            {
                foreach (var headerParam in options.HeaderParameters)
                {
                    foreach (var value in headerParam.Value)
                    {
                        request.SetRequestHeader(headerParam.Key, value);
                    }
                }
            }

            request.timeout = configuration.Timeout;

            if (options.Data != null)
            {
                var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                var data = IsolatedJsonConvert.SerializeObject(options.Data, settings);
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            return request;
        }
    }
}
