using System.Collections.Generic;
using System.Threading.Tasks;

namespace Unity.Services.Authentication.PlayerAccounts
{
    interface INetworkHandler
    {
        Task<T> GetAsync<T>(string url, IDictionary<string, string> headers = null);

        Task<T> PostAsync<T>(string url, IDictionary<string, string> headers = null);

        Task<T> PostAsync<T>(string url, string payload, IDictionary<string, string> headers = null);

        Task<T> PutAsync<T>(string url, object payload, IDictionary<string, string> headers = null);

        Task DeleteAsync(string url, IDictionary<string, string> headers = null);

        Task<T> DeleteAsync<T>(string url, IDictionary<string, string> headers = null);
    }
}
