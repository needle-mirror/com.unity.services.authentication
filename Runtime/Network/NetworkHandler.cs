using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Unity.Services.Authentication
{
    class NetworkHandler : INetworkHandler
    {
        public static class ContentType
        {
            public const string Json = "application/json";
        }

        INetworkConfiguration Configuration { get; }

        public NetworkHandler()
        {
            Configuration = new NetworkConfiguration();
        }

        public Task<T> GetAsync<T>(string url, IDictionary<string, string> headers = null)
        {
            var request = new WebRequest(
                Configuration,
                WebRequestVerb.Get,
                url,
                headers,
                null,
                ContentType.Json);

            return request.SendAsync<T>();
        }

        public Task<T> PostAsync<T>(string url, IDictionary<string, string> headers = null)
        {
            var request = new WebRequest(
                Configuration,
                WebRequestVerb.Post,
                url,
                headers,
                null,
                ContentType.Json);

            return request.SendAsync<T>();
        }

        public Task<T> PostAsync<T>(string url, object payload, IDictionary<string, string> headers = null)
        {
            var jsonPayload = payload != null ? JsonConvert.SerializeObject(payload) : null;

            var request = new WebRequest(
                Configuration,
                WebRequestVerb.Post,
                url,
                headers,
                jsonPayload,
                ContentType.Json);

            return request.SendAsync<T>();
        }

        public Task<T> PutAsync<T>(string url, object payload, IDictionary<string, string> headers = null)
        {
            var jsonPayload = payload != null ? JsonConvert.SerializeObject(payload) : null;

            var request = new WebRequest(
                Configuration,
                WebRequestVerb.Put,
                url,
                headers,
                jsonPayload,
                ContentType.Json);

            return request.SendAsync<T>();
        }

        public Task DeleteAsync(string url, IDictionary<string, string> headers = null)
        {
            var request = new WebRequest(
                Configuration,
                WebRequestVerb.Delete,
                url,
                headers,
                null,
                ContentType.Json);

            return request.SendAsync();
        }

        public Task<T> DeleteAsync<T>(string url, IDictionary<string, string> headers = null)
        {
            var request = new WebRequest(
                Configuration,
                WebRequestVerb.Delete,
                url,
                headers,
                null,
                ContentType.Json);

            return request.SendAsync<T>();
        }
    }
}
