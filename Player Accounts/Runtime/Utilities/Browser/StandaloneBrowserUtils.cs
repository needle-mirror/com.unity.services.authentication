#if UNITY_EDITOR || UNITY_STANDALONE
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    class StandaloneBrowserUtils : IBrowserUtils
    {
        public event Action<string> AuthCodeReceivedEvent;

        HttpListener m_HttpListener;
        int? m_BoundPort = null;

        public StandaloneBrowserUtils()
        {
        }

        public string GetRedirectUri()
        {
            return $"http://localhost:{m_BoundPort}/callback";
        }

        public bool Bind()
        {
            if (m_BoundPort.HasValue)
            {
                return true;
            }

            if (HttpUtilities.TryBindListenerOnFreePort(out var httpListener, out var port))
            {
                m_HttpListener = httpListener;
                m_BoundPort = port;
                return true;
            }

            return false;
        }

        public async Task LaunchUrlAsync(string url)
        {
            Application.OpenURL(url);

            if (m_HttpListener.IsListening)
            {
                m_HttpListener.Stop();
            }

            m_HttpListener.Start();
            HttpListenerContext context;

            try
            {
                context = await m_HttpListener.GetContextAsync();
            }
            catch (ObjectDisposedException ex)
            {
                Logger.Log("HttpListener has been disposed." + ex.Message);
                return;
            }
            catch (Exception ex)
            {
                Logger.Log("HttpListener error." + ex.Message);
                return;
            }

            SendBrowserResponse(context.Response, m_HttpListener);
            m_HttpListener.Stop();

            var uri = new Uri(url);
            var queryParameters = UriHelper.ParseQueryString(uri.Query);

            if (!queryParameters.TryGetValue("state", out var state) || state == null)
            {
                throw PlayerAccountsExceptionHandler.HandleError("State parameter not found in URL", nameof(state));
            }

            var code = GetAuthCode(context, state);
            AuthCodeReceivedEvent?.Invoke(code);
        }

        public void Dismiss()
        {
        }

        static void SendBrowserResponse(HttpListenerResponse response, HttpListener http)
        {
            var responseString = "<html><body><b>DONE!</b><br>(You can return to your app and close this tab/window now)";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            var responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith(_ =>
            {
                responseOutput.Close();
                http.Stop();
            });
        }

        static string GetAuthCode(HttpListenerContext context, string state)
        {
            var code = context.Request.QueryString.Get("code");
            var error = context.Request.QueryString.Get("error");
            var incomingState = context.Request.QueryString.Get("state");
            var uri = new Uri(context.Request.Url.AbsoluteUri);

            if (!string.IsNullOrEmpty(error))
            {
                throw PlayerAccountsExceptionHandler.HandleError(error);
            }

            if (string.IsNullOrEmpty(code))
            {
                var fragment = UriHelper.ParseQueryString(uri.Fragment);
                code = fragment["code"];
                incomingState = fragment["state"];
            }

            if (incomingState != state)
            {
                throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidState, $"Received request with invalid state ({incomingState})");
            }

            return code;
        }
    }
}
#endif
