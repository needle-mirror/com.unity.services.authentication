#if UNITY_PIPELINE_AVAILABLE
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Unity.Services.Authentication.Editor;

namespace Unity.Services.Authentication.Cli.Tests
{
    class PlayerAccountsAdminClientTests
    {
        const string k_Host = "https://services.example.test";
        const string k_ProjectId = "project-1";

        FakeNetworkHandler m_Network;
        FakeAuthenticationAdmin m_AuthenticationAdmin;
        FakeGenesisTokenProvider m_Genesis;
        PlayerAccountsAdminClient m_Client;

        [SetUp]
        public void SetUp()
        {
            m_Network = new FakeNetworkHandler();
            m_AuthenticationAdmin = new FakeAuthenticationAdmin();
            m_Genesis = new FakeGenesisTokenProvider { Token = "genesis" };
            m_Client = new PlayerAccountsAdminClient(k_Host, k_ProjectId, m_Network, m_AuthenticationAdmin, m_Genesis);
        }

        static T Run<T>(Task<T> task) => task.GetAwaiter().GetResult();

        [Test]
        public void CreateOrUpdate_PostsToTheProjectsClientsUrlWithTheGatewayToken()
        {
            var request = new PlayerAccountsClientRequest { Name = "My Game", Platforms = new[] { "mobile" } };

            m_Client.CreateOrUpdateClientAsync(request).GetAwaiter().GetResult();

            Assert.That(m_Network.LastMethod, Is.EqualTo("POST"));
            Assert.That(m_Network.LastPayload, Is.SameAs(request));
            Assert.That(m_Network.LastUrl, Is.EqualTo(k_Host + "/api/player-accounts/v1/projects/" + k_ProjectId + "/clients"));
            Assert.That(m_Network.LastHeaders["Authorization"], Is.EqualTo("Bearer gateway"));
            Assert.That(m_Network.LastHeaders["ProjectId"], Is.EqualTo(k_ProjectId));
            Assert.That(m_AuthenticationAdmin.ExchangedToken, Is.EqualTo("genesis"));
        }

        [Test]
        public void Get_EscapesTheClientIdInTheUrl()
        {
            m_Network.Response = new PlayerAccountsClientResponse { ClientId = "a b" };

            Run(m_Client.GetClientAsync("a b"));

            Assert.That(m_Network.LastMethod, Is.EqualTo("GET"));
            Assert.That(m_Network.LastUrl, Is.EqualTo(m_Client.ClientsUrl + "/a%20b"));
        }

        [Test]
        public void Update_PutsTheRequestToTheClientsUrl()
        {
            var request = new PlayerAccountsClientRequest { Name = "n", Platforms = new[] { "desktop" } };
            m_Network.Response = new PlayerAccountsClientResponse { ClientId = "client-1" };

            Run(m_Client.UpdateClientAsync("client-1", request));

            Assert.That(m_Network.LastMethod, Is.EqualTo("PUT"));
            Assert.That(m_Network.LastUrl, Is.EqualTo(m_Client.ClientsUrl + "/client-1"));
            Assert.That(m_Network.LastPayload, Is.SameAs(request));
        }

        [Test]
        public void GetUnitySignInProvider_FindsTheUnityEntryInAuthentication()
        {
            m_AuthenticationAdmin.Providers = new ListIdProviderResponse
            {
                Results = new[]
                {
                    new IdProviderResponse { Type = "apple.com", ClientId = "apple" },
                    new IdProviderResponse { Type = "unity", ClientId = "client-1", Disabled = true },
                },
            };

            var provider = Run(m_Client.GetUnitySignInProviderAsync());

            Assert.That(provider.ClientId, Is.EqualTo("client-1"));
            Assert.That(provider.Disabled, Is.True);
            Assert.That(m_AuthenticationAdmin.LastProjectId, Is.EqualTo(k_ProjectId));
            Assert.That(m_AuthenticationAdmin.LastToken, Is.EqualTo("gateway"));
        }

        [Test]
        public void GetUnitySignInProvider_WithoutTheUnityEntry_ReturnsNull()
        {
            m_AuthenticationAdmin.Providers = new ListIdProviderResponse { Results = new[] { new IdProviderResponse { Type = "apple.com" } } };

            Assert.That(Run(m_Client.GetUnitySignInProviderAsync()), Is.Null);
        }

        [Test]
        public void EnableUnitySignInProvider_EnablesTheUnityType()
        {
            Run(m_Client.EnableUnitySignInProviderAsync());

            Assert.That(m_AuthenticationAdmin.EnabledType, Is.EqualTo("unity"));
        }

        [Test]
        public void IdentityProviderCalls_UseTheIdpsUrls()
        {
            Assert.That(m_Client.IdentityProvidersUrl, Is.EqualTo(k_Host + "/api/player-accounts/v1/projects/" + k_ProjectId + "/idps"));

            m_Network.Response = new ListPlayerAccountsIdentityProvidersResponse();
            Assert.That(Run(m_Client.ListIdentityProvidersAsync()), Is.Empty);
            Assert.That(m_Network.LastUrl, Is.EqualTo(m_Client.IdentityProvidersUrl));

            var request = new CreatePlayerAccountsIdentityProviderRequest { Provider = "apple", ProviderClientId = "com.example" };
            m_Network.Response = new PlayerAccountsIdentityProviderResponse { Id = "idp-1" };
            Run(m_Client.CreateIdentityProviderAsync(request));
            Assert.That((m_Network.LastMethod, m_Network.LastUrl), Is.EqualTo(("POST", m_Client.IdentityProvidersUrl)));
            Assert.That(m_Network.LastPayload, Is.SameAs(request));

            var update = new UpdatePlayerAccountsIdentityProviderRequest { Disabled = true };
            Run(m_Client.UpdateIdentityProviderAsync("idp-1", update));
            Assert.That((m_Network.LastMethod, m_Network.LastUrl), Is.EqualTo(("PUT", m_Client.IdentityProvidersUrl + "/idp-1")));
            Assert.That(m_Network.LastPayload, Is.SameAs(update));

            m_Client.DeleteIdentityProviderAsync("idp-1").GetAwaiter().GetResult();
            Assert.That((m_Network.LastMethod, m_Network.LastUrl), Is.EqualTo(("DELETE", m_Client.IdentityProvidersUrl + "/idp-1")));
        }

        [Test]
        public void Requests_SerializeToTheSpecsFieldNamesAndLeaveOutUnsetFields()
        {
            var update = new PlayerAccountsClientRequest { Platforms = new[] { "desktop" }, PrivacyPolicyLink = "" };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(update);
            Assert.That(json, Is.EqualTo("{\"platforms\":[\"desktop\"],\"privacyPolicyLink\":\"\"}"));

            var idp = new CreatePlayerAccountsIdentityProviderRequest { Provider = "apple", ProviderClientId = "com.example" };
            Assert.That(Newtonsoft.Json.JsonConvert.SerializeObject(idp), Is.EqualTo("{\"provider\":\"apple\",\"providerClientId\":\"com.example\"}"));

            var toggle = new UpdatePlayerAccountsIdentityProviderRequest { Disabled = false };
            Assert.That(Newtonsoft.Json.JsonConvert.SerializeObject(toggle), Is.EqualTo("{\"disabled\":false}"));
        }

        [TestCase(401, PlayerAccountsAdminException.FailureReason.Unauthorized)]
        [TestCase(403, PlayerAccountsAdminException.FailureReason.Forbidden)]
        [TestCase(404, PlayerAccountsAdminException.FailureReason.NotFound)]
        [TestCase(400, PlayerAccountsAdminException.FailureReason.InvalidRequest)]
        [TestCase(500, PlayerAccountsAdminException.FailureReason.Unknown)]
        public void FailedCall_IsClassifiedByStatusCode(long status, PlayerAccountsAdminException.FailureReason expected)
        {
            m_Network.Failure = new WebRequestException(false, true, false, status,
                "{\"title\":\"FORBIDDEN\",\"detail\":\"Missing permission\",\"status\":" + status + "}");

            var error = Assert.Throws<PlayerAccountsAdminException>(() => Run(m_Client.GetClientAsync("client-1")));

            Assert.That(error.Reason, Is.EqualTo(expected));
            Assert.That(error.StatusCode, Is.EqualTo(status));
            Assert.That(error.ServerDetail, Is.EqualTo("Missing permission"));
        }

        [Test]
        public void FailedAuthenticationCall_IsClassifiedTheSameWay()
        {
            m_AuthenticationAdmin.Failure = new WebRequestException(false, true, false, 403, "{\"detail\":\"Missing permission\"}");

            var error = Assert.Throws<PlayerAccountsAdminException>(() => Run(m_Client.GetUnitySignInProviderAsync()));

            Assert.That(error.Reason, Is.EqualTo(PlayerAccountsAdminException.FailureReason.Forbidden));
        }

        [Test]
        public void NetworkFailure_IsClassifiedAsNetwork()
        {
            m_Network.Failure = new WebRequestException(true, false, false, 0, "Cannot resolve destination host");

            var error = Assert.Throws<PlayerAccountsAdminException>(() => Run(m_Client.ListIdentityProvidersAsync()));

            Assert.That(error.Reason, Is.EqualTo(PlayerAccountsAdminException.FailureReason.Network));
        }

        [Test]
        public void Unauthorized_DropsTheGatewayTokenSoTheNextCallExchangesAgain()
        {
            m_Network.Failure = new WebRequestException(false, true, false, 401, "");
            Assert.Throws<PlayerAccountsAdminException>(() => Run(m_Client.ListIdentityProvidersAsync()));

            m_Network.Failure = null;
            m_Network.Response = new ListPlayerAccountsIdentityProvidersResponse();
            Run(m_Client.ListIdentityProvidersAsync());

            Assert.That(m_AuthenticationAdmin.Exchanges, Is.EqualTo(2));
        }

        [Test]
        public void RefusedTokenExchange_IsUnauthorized()
        {
            m_AuthenticationAdmin.ExchangeFailure = new WebRequestException(false, true, false, 401, "{\"detail\":\"Token expired\"}");

            var error = Assert.Throws<PlayerAccountsAdminException>(() => Run(m_Client.GetClientAsync("client-1")));

            Assert.That(error.Reason, Is.EqualTo(PlayerAccountsAdminException.FailureReason.Unauthorized));
            Assert.That(error.ServerDetail, Is.EqualTo("Token expired"));
            Assert.That(m_Network.Calls, Is.Zero);
        }

        [Test]
        public void FailedTokenExchange_IsAServiceFailureNotAnExpiredSignIn()
        {
            m_AuthenticationAdmin.ExchangeFailure = new WebRequestException(false, true, false, 503, "{\"detail\":\"Service unavailable\"}");

            var error = Assert.Throws<PlayerAccountsAdminException>(() => Run(m_Client.GetClientAsync("client-1")));

            Assert.That(error.Reason, Is.EqualTo(PlayerAccountsAdminException.FailureReason.Unknown));
            Assert.That(error.StatusCode, Is.EqualTo(503));
            Assert.That(error.ServerDetail, Is.EqualTo("Service unavailable"));
        }

        [Test]
        public void SignedOutEditor_IsUnauthorizedWithoutCallingTheService()
        {
            m_Genesis.Token = null;

            var error = Assert.Throws<PlayerAccountsAdminException>(() => Run(m_Client.GetClientAsync("client-1")));

            Assert.That(error.Reason, Is.EqualTo(PlayerAccountsAdminException.FailureReason.Unauthorized));
            Assert.That(m_AuthenticationAdmin.Exchanges, Is.Zero);
            Assert.That(m_Network.Calls, Is.Zero);
        }

        [Test]
        public void ServerDetail_FallsBackToTheRawBodyWhenItIsNotProblemDetails()
        {
            Assert.That(PlayerAccountsAdminClient.ServerDetail("plain text"), Is.EqualTo("plain text"));
            Assert.That(PlayerAccountsAdminClient.ServerDetail(null), Is.Null);
        }

        class FakeNetworkHandler : INetworkHandler
        {
            public object Response;
            public Exception Failure;
            public string LastMethod;
            public string LastUrl;
            public object LastPayload;
            public IDictionary<string, string> LastHeaders;
            public int Calls;

            Task<T> Respond<T>(string method, string url, object payload, IDictionary<string, string> headers)
            {
                Calls++;
                LastMethod = method;
                LastUrl = url;
                LastPayload = payload;
                LastHeaders = headers;
                return Failure != null ? Task.FromException<T>(Failure) : Task.FromResult(Response is T typed ? typed : default);
            }

            public Task<T> GetAsync<T>(string url, IDictionary<string, string> headers = null) => Respond<T>("GET", url, null, headers);
            public Task<T> PostAsync<T>(string url, IDictionary<string, string> headers = null) => Respond<T>("POST", url, null, headers);
            public Task<T> PostAsync<T>(string url, object payload, IDictionary<string, string> headers = null) => Respond<T>("POST", url, payload, headers);
            public Task<T> PutAsync<T>(string url, object payload, IDictionary<string, string> headers = null) => Respond<T>("PUT", url, payload, headers);
            public Task DeleteAsync(string url, IDictionary<string, string> headers = null) => Respond<object>("DELETE", url, null, headers);
            public Task<T> DeleteAsync<T>(string url, IDictionary<string, string> headers = null) => Respond<T>("DELETE", url, null, headers);
        }

        class FakeAuthenticationAdmin : IAuthenticationAdminNetworkClient
        {
            public ListIdProviderResponse Providers = new ListIdProviderResponse();
            public Exception Failure;
            public Exception ExchangeFailure;
            public string ExchangedToken;
            public int Exchanges;
            public string LastProjectId;
            public string LastToken;
            public string EnabledType;

            public Task<TokenExchangeResponse> ExchangeTokenAsync(string token)
            {
                Exchanges++;
                ExchangedToken = token;
                return ExchangeFailure != null ? Task.FromException<TokenExchangeResponse>(ExchangeFailure) : Task.FromResult(new TokenExchangeResponse { Token = "gateway" });
            }

            public Task<ListIdProviderResponse> ListIdProviderAsync(string projectId, string token)
            {
                LastProjectId = projectId;
                LastToken = token;
                return Failure != null ? Task.FromException<ListIdProviderResponse>(Failure) : Task.FromResult(Providers);
            }

            public Task<IdProviderResponse> EnableIdProviderAsync(string projectId, string type, string token)
            {
                LastProjectId = projectId;
                EnabledType = type;
                return Task.FromResult(new IdProviderResponse { Type = type });
            }

            public Task<IdProviderResponse> CreateIdProviderAsync(CreateIdProviderRequest body, string projectId, string token) => throw new NotSupportedException();
            public Task<IdProviderResponse> UpdateIdProviderAsync(UpdateIdProviderRequest body, string projectId, string type, string token) => throw new NotSupportedException();
            public Task<IdProviderResponse> DisableIdProviderAsync(string projectId, string type, string token) => throw new NotSupportedException();
            public Task<IdProviderResponse> DeleteIdProviderAsync(string projectId, string type, string token) => throw new NotSupportedException();
        }

        class FakeGenesisTokenProvider : IGenesisTokenProvider
        {
            public string Token { get; set; }
        }
    }
}
#endif
