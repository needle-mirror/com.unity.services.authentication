using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    class NetworkHandler : INetworkHandler
    {
        public static class ContentType
        {
            public const string Json = "application/json";
        }

        INetworkConfiguration Configuration { get; }
        readonly JsonSerializerSettings m_JsonSerializerSettings = new JsonSerializerSettings();

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

        public Task<T> PostAsync<T>(string url, string payload, IDictionary<string, string> headers = null)
        {
            var request = new WebRequest(
                Configuration,
                WebRequestVerb.Post,
                url,
                headers,
                payload,
                "application/x-www-form-urlencoded");

            return request.SendAsync<T>();
        }

        public Task<T> PutAsync<T>(string url, object payload, IDictionary<string, string> headers = null)
        {
            var jsonPayload = payload != null ? JsonConvert.SerializeObject(payload, m_JsonSerializerSettings) : null;

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
