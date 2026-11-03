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
        const string k_DoneResponse = "<html><body><b>DONE!</b><br>(You can return to your app and close this tab/window now)";
        const string k_StaleResponse = "<html><body>This sign-in request is no longer active. You can close this tab/window now.";

        public event Action<string> AuthCodeReceivedEvent;

        readonly Action<string> m_OpenUrl;
        HttpListener m_HttpListener;
        int? m_BoundPort = null;

        // Bumped by every launch and every cancel, so a listening loop from an earlier launch stops instead of
        // competing with the current one for requests.
        int m_Attempt;

        public StandaloneBrowserUtils() : this(Application.OpenURL)
        {
        }

        internal StandaloneBrowserUtils(Action<string> openUrl)
        {
            m_OpenUrl = openUrl;
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
            var attempt = ++m_Attempt;

            var queryParameters = UriHelper.ParseQueryString(new Uri(url).Query);

            if (!queryParameters.TryGetValue("state", out var state) || state == null)
            {
                throw PlayerAccountsExceptionHandler.HandleError("State parameter not found in URL", nameof(state));
            }

            if (m_HttpListener.IsListening)
            {
                m_HttpListener.Stop();
            }

            m_HttpListener.Start();
            m_OpenUrl(url);

            while (true)
            {
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

                if (attempt != m_Attempt || !IsForState(context.Request, state))
                {
                    // A callback from an abandoned sign-in tab, or any other stray request.
                    Logger.Log("Ignoring a sign-in callback that doesn't belong to the current sign-in attempt.");
                    SendBrowserResponse(context.Response, k_StaleResponse);

                    if (attempt != m_Attempt)
                    {
                        return;
                    }

                    continue;
                }

                SendBrowserResponse(context.Response, k_DoneResponse);
                m_HttpListener.Stop();

                var code = GetAuthCode(context.Request);
                AuthCodeReceivedEvent?.Invoke(code);
                return;
            }
        }

        public void Cancel()
        {
            m_Attempt++;

            if (m_HttpListener != null && m_HttpListener.IsListening)
            {
                m_HttpListener.Stop();
            }
        }

        static bool IsForState(HttpListenerRequest request, string state)
        {
            var incomingState = request.QueryString.Get("state");

            if (incomingState == null)
            {
                // An error redirect without state can only be acted on as belonging to the current attempt;
                // ignoring it would leave sign-in waiting forever.
                return !string.IsNullOrEmpty(request.QueryString.Get("error"));
            }

            return incomingState == state;
        }

        static void SendBrowserResponse(HttpListenerResponse response, string responseString)
        {
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith(_ => responseOutput.Close());
        }

        static string GetAuthCode(HttpListenerRequest request)
        {
            var error = request.QueryString.Get("error");

            if (!string.IsNullOrEmpty(error))
            {
                throw PlayerAccountsExceptionHandler.HandleError(error);
            }

            var code = request.QueryString.Get("code");

            if (string.IsNullOrEmpty(code))
            {
                throw PlayerAccountsException.Create(PlayerAccountsErrorCodes.InvalidRequest, "Sign-in callback has no authorization code.");
            }

            return code;
        }
    }
}
#endif
