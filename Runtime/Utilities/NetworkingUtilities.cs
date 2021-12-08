using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Services.Core.Scheduler.Internal;

namespace Unity.Services.Authentication.Utilities
{
    interface INetworkingUtilities
    {
        Task<T> GetAsync<T>(string url, IDictionary<string, string> headers = null, int maximumAttempts = 1);

        Task<T> PostJsonAsync<T>(string url, object payload, IDictionary<string, string> headers = null,
            int maximumAttempts = 1);

        Task<T> PostFormAsync<T>(string url, string payload, IDictionary<string, string> headers = null,
            int maximumAttempts = 1);

        Task<T> PostAsync<T>(string url, IDictionary<string, string> headers = null, int maximumAttempts = 1);

        Task<T> PutAsync<T>(string url, object payload, IDictionary<string, string> headers = null, int maximumAttempts = 1);

        Task DeleteAsync(string url, IDictionary<string, string> headers = null, int maximumAttempts = 1);

        Task<T> DeleteAsync<T>(string url, IDictionary<string, string> headers = null, int maximumAttempts = 1);
    }

    class NetworkingUtilities : INetworkingUtilities
    {
        readonly IActionScheduler m_Scheduler;

        /// <summary>
        /// The max redirect to follow. By default it's set to 0 and returns the raw 3xx response with a location header.
        /// </summary>
        public int RedirectLimit { get; set; }

        public NetworkingUtilities(IActionScheduler scheduler)
        {
            m_Scheduler = scheduler;
        }

        public Task<T> GetAsync<T>(string url, IDictionary<string, string> headers = null, int maximumAttempts = 1)
        {
            var request = new WebRequest(m_Scheduler,
                WebRequestVerb.Get,
                url,
                headers,
                string.Empty,
                string.Empty,
                RedirectLimit,
                maximumAttempts);

            if (ThreadHelper.IsMainThread || m_Scheduler == null)
            {
                return request.SendAsync<T>();
            }
            else
            {
                return ScheduleWebRequest<T>(request);
            }
        }

        public Task<T> PostAsync<T>(string url, IDictionary<string, string> headers = null, int maximumAttempts = 1)
        {
            var request = new WebRequest(m_Scheduler,
                WebRequestVerb.Post,
                url,
                headers,
                string.Empty,
                string.Empty,
                RedirectLimit,
                maximumAttempts);

            if (ThreadHelper.IsMainThread || m_Scheduler == null)
            {
                return request.SendAsync<T>();
            }
            else
            {
                return ScheduleWebRequest<T>(request);
            }
        }

        public Task<T> PostJsonAsync<T>(string url, object payload, IDictionary<string, string> headers = null, int maximumAttempts = 1)
        {
            var jsonPayload = JsonConvert.SerializeObject(payload);

            var request = new WebRequest(m_Scheduler,
                WebRequestVerb.Post,
                url,
                headers,
                jsonPayload,
                "application/json",
                RedirectLimit,
                maximumAttempts);

            if (ThreadHelper.IsMainThread || m_Scheduler == null)
            {
                return request.SendAsync<T>();
            }
            else
            {
                return ScheduleWebRequest<T>(request);
            }
        }

        public Task<T> PostFormAsync<T>(string url, string payload, IDictionary<string, string> headers = null, int maximumAttempts = 1)
        {
            var request = new WebRequest(m_Scheduler,
                WebRequestVerb.Post,
                url,
                headers,
                payload,
                "application/x-www-form-urlencoded",
                RedirectLimit,
                maximumAttempts);

            if (ThreadHelper.IsMainThread || m_Scheduler == null)
            {
                return request.SendAsync<T>();
            }
            else
            {
                return ScheduleWebRequest<T>(request);
            }
        }

        public Task<T> PutAsync<T>(string url, object payload, IDictionary<string, string> headers = null, int maximumAttempts = 1)
        {
            var jsonPayload = JsonConvert.SerializeObject(payload);

            var request = new WebRequest(m_Scheduler,
                WebRequestVerb.Put,
                url,
                headers,
                jsonPayload,
                "application/json",
                RedirectLimit,
                maximumAttempts);

            if (ThreadHelper.IsMainThread || m_Scheduler == null)
            {
                return request.SendAsync<T>();
            }
            else
            {
                return ScheduleWebRequest<T>(request);
            }
        }

        public Task DeleteAsync(string url, IDictionary<string, string> headers = null, int maximumAttempts = 1)
        {
            var request = new WebRequest(m_Scheduler,
                WebRequestVerb.Delete,
                url,
                headers,
                string.Empty,
                string.Empty,
                RedirectLimit,
                maximumAttempts);

            if (ThreadHelper.IsMainThread || m_Scheduler == null)
            {
                return request.SendAsync();
            }
            else
            {
                return ScheduleWebRequest(request);
            }
        }

        public Task<T> DeleteAsync<T>(string url, IDictionary<string, string> headers = null, int maximumAttempts = 1)
        {
            var request = new WebRequest(m_Scheduler,
                WebRequestVerb.Delete,
                url,
                headers,
                string.Empty,
                string.Empty,
                RedirectLimit,
                maximumAttempts);

            if (ThreadHelper.IsMainThread || m_Scheduler == null)
            {
                return request.SendAsync<T>();
            }
            else
            {
                return ScheduleWebRequest<T>(request);
            }
        }

        Task ScheduleWebRequest(WebRequest request)
        {
            var tcs = new TaskCompletionSource<object>();

            m_Scheduler.ScheduleAction(async() =>
            {
                try
                {
                    await request.SendAsync();
                    tcs.SetResult(null);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }

        Task<T> ScheduleWebRequest<T>(WebRequest request)
        {
            var tcs = new TaskCompletionSource<T>();

            m_Scheduler.ScheduleAction(async() =>
            {
                try
                {
                    tcs.SetResult(await request.SendAsync<T>());
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });

            return tcs.Task;
        }
    }
}
