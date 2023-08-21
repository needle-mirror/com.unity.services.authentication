using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Authentication.Server.Shared;
using UnityEngine.Networking;

namespace Unity.Services.Authentication.Server
{
    class AuthenticationServerApiClient : IApiClient
    {
        const string k_MethodGet = "GET";
        const string k_MethodHead = "HEAD";
        const string k_MethodPost = "POST";
        const string k_MethodPut = "PUT";
        const string k_MethodPatch = "PATCH";
        const string k_MethodOptions = "OPTIONS";
        const string k_MethodDelete = "DELETE";

        IServerConfiguration Configuration { get; }

        public AuthenticationServerApiClient(IServerConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Task<ApiResponse> GetAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, k_MethodGet, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> GetAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, k_MethodGet, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> PostAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, k_MethodPost, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> PostAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, k_MethodPost, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> PutAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, k_MethodPut, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> PutAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, k_MethodPut, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> DeleteAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, k_MethodDelete, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> DeleteAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, k_MethodDelete, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> HeadAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, k_MethodHead, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> HeadAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, k_MethodHead, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> OptionsAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, k_MethodOptions, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> OptionsAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, k_MethodOptions, options, configuration, cancellationToken);
        }

        public Task<ApiResponse> PatchAsync(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync(path, k_MethodPatch, options, configuration, cancellationToken);
        }

        public Task<ApiResponse<T>> PatchAsync<T>(string path, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            return SendAsync<T>(path, k_MethodPatch, options, configuration, cancellationToken);
        }

        async Task<ApiResponse> SendAsync(string path, string method, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            bool shouldRetry;
            var attempts = 0;

            do
            {
                try
                {
                    using (var request = BuildWebRequest(path, method, options, configuration))
                    {
                        return await WebRequestUtils.SendWebRequestAsync(request);
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

        async Task<ApiResponse<T>> SendAsync<T>(string path, string method, ApiRequestOptions options, IApiConfiguration configuration, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            bool shouldRetry;
            var attempts = 0;

            do
            {
                try
                {
                    using (var request = BuildWebRequest(path, method, options, configuration))
                    {
                        return await WebRequestUtils.SendWebRequestAsync<T>(request, cancellationToken);
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

        internal UnityWebRequest BuildWebRequest(string path, string method, ApiRequestOptions options, IApiConfiguration configuration)
        {
            var builder = new ApiRequestPathBuilder(configuration.BasePath, path);
            builder.AddPathParameters(options.PathParameters);
            builder.AddQueryParameters(options.QueryParameters);
            var uri = builder.GetFullUri();

            var request = new UnityWebRequest(uri, method);

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
                var data = JsonConvert.SerializeObject(options.Data, settings);
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            return request;
        }
    }
}
