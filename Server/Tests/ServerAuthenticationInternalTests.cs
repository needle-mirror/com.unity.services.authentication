#if NUGET_MOQ_AVAILABLE && UNITY_EDITOR
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Authentication.Server.Environments.Generated;
using Unity.Services.Authentication.Server.Proxy.Generated;
using Unity.Services.Authentication.Server.ServiceAuth.Generated;
using Unity.Services.Authentication.Server.Shared;
using Unity.Services.Authentication.Tests;
using Unity.Services.Core.Configuration.Internal;
using Unity.Services.Core.Environments.Internal;
using Unity.Services.Core.Scheduler.Internal;
using UnityEngine.TestTools;

namespace Unity.Services.Authentication.Server.Tests
{
    class ServerAuthenticationInternalTests
    {
        ServerAuthenticationServiceInternal m_Authentication;

        ServerAccessTokenComponent m_AccessTokenComponent;
        ServerEnvironmentIdComponent m_EnvironmentIdComponent;
        ApiConfiguration m_Configuration;

        Mock<IServerAuthenticationSettings> m_MockSettings;
        Mock<IAuthenticationNetworkClient> m_MockNetwork;
        Mock<IJwtDecoder> m_MockJwtDecoder;
        Mock<IActionScheduler> m_MockScheduler;
        Mock<IDateTimeWrapper> m_MockDateTime;
        Mock<ICloudProjectId> m_MockCloudProjectId;
        Mock<IEnvironments> m_MockEnvironment;
        Mock<IServerConfiguration> m_MockServerConfiguration;
        Mock<IEnvironmentApi> m_MockEnvironmentApi;
        Mock<IServiceAuthenticationApi> m_MockServiceAuthApi;
        Mock<IProxyApi> m_MockProxyApi;

        const long k_ExpirationActionId = 1234;
        const long k_RefreshActionId = 3456;

        [SetUp]
        public void SetUp()
        {
            m_AccessTokenComponent = new ServerAccessTokenComponent();
            m_EnvironmentIdComponent = new ServerEnvironmentIdComponent();
            m_Configuration = new ApiConfiguration();

            m_MockSettings = new Mock<IServerAuthenticationSettings>();
            m_MockNetwork = new Mock<IAuthenticationNetworkClient>();
            m_MockCloudProjectId = new Mock<ICloudProjectId>();
            m_MockEnvironment = new Mock<IEnvironments>();
            m_MockJwtDecoder = new Mock<IJwtDecoder>();
            m_MockScheduler = new Mock<IActionScheduler>();
            m_MockDateTime = new Mock<IDateTimeWrapper>();
            m_MockServerConfiguration = new Mock<IServerConfiguration>();
            m_MockEnvironmentApi = new Mock<IEnvironmentApi>();
            m_MockServiceAuthApi = new Mock<IServiceAuthenticationApi>();
            m_MockProxyApi = new Mock<IProxyApi>();

            m_Authentication = new ServerAuthenticationServiceInternal(
                m_MockSettings.Object,
                m_MockCloudProjectId.Object,
                m_MockJwtDecoder.Object,
                m_MockScheduler.Object,
                m_MockDateTime.Object,
                m_MockEnvironment.Object,
                m_AccessTokenComponent,
                m_EnvironmentIdComponent,
                m_MockServerConfiguration.Object,
                m_MockEnvironmentApi.Object,
                m_MockServiceAuthApi.Object,
                m_MockProxyApi.Object);
        }

        [UnityTest]
        public IEnumerator RefreshAccessTokenAsync_IsNotSignedIn_DoesNothing() =>
            AsyncTest.AsCoroutine(RefreshAccessTokenAsync_IsNotSignedIn_DoesNothingAsync);
        async Task RefreshAccessTokenAsync_IsNotSignedIn_DoesNothingAsync()
        {
            m_Authentication.State = ServerAuthenticationState.Unauthorized;

            await m_Authentication.StartRefreshAsync();

            m_MockNetwork.VerifyAll();
            Assert.AreEqual(ServerAuthenticationState.Unauthorized, m_Authentication.State);
        }

        [UnityTest]
        public IEnumerator RefreshAccessTokenAsync_IsSignedIn_SendsRefreshRequest() =>
            AsyncTest.AsCoroutine(RefreshAccessTokenAsync_IsSignedIn_SendsRefreshRequestAsync);
        async Task RefreshAccessTokenAsync_IsSignedIn_SendsRefreshRequestAsync()
        {
            m_Authentication.State = ServerAuthenticationState.Authorized;

            await m_Authentication.StartRefreshAsync();

            m_MockNetwork.VerifyAll();

            Assert.AreEqual(ServerAuthenticationState.Authorized, m_Authentication.State);
        }

        [UnityTest]
        public IEnumerator RefreshAccessTokenAsync_IsExpired_DoesNothing() =>
            AsyncTest.AsCoroutine(RefreshAccessTokenAsync_IsExpired_DoesNothingAsync);
        async Task RefreshAccessTokenAsync_IsExpired_DoesNothingAsync()
        {
            m_Authentication.State = ServerAuthenticationState.Expired;
            await m_Authentication.StartRefreshAsync();
            Assert.AreEqual(ServerAuthenticationState.Expired, m_Authentication.State);
        }

        [Test]
        public void ScheduleExpiration_Executes_IdMatches()
        {
            var delay = 0d;

            m_MockScheduler.Setup(n => n.ScheduleAction(m_Authentication.ExecuteScheduledExpiration, delay)).Returns(k_ExpirationActionId);

            m_Authentication.ScheduleExpiration(delay);

            Assert.AreEqual(k_ExpirationActionId, m_Authentication.ExpirationActionId);
            m_MockScheduler.VerifyAll();
        }

        [Test]
        public void ScheduleRefresh_Executes_IdMatches()
        {
            var delay = 1d;
            var utcNow = DateTime.UtcNow;
            var expiryTime = utcNow + TimeSpan.FromSeconds(10);


            m_AccessTokenComponent.ExpiryTime = expiryTime;
            m_MockDateTime.Setup(n => n.UtcNow).Returns(utcNow);
            m_MockScheduler.Setup(n => n.ScheduleAction(m_Authentication.ExecuteScheduledRefresh, delay)).Returns(k_RefreshActionId);

            m_Authentication.ScheduleRefresh(delay);

            Assert.AreEqual(k_RefreshActionId, m_Authentication.RefreshActionId);

            m_MockDateTime.VerifyAll();
            m_MockScheduler.VerifyAll();
        }

        [Test]
        public void ScheduleRefresh_AfterExpiry_DoesNothing()
        {
            var delay = 1d;
            var now = DateTime.UtcNow;
            m_Authentication.RefreshActionId = null;

            m_AccessTokenComponent.ExpiryTime = now;
            m_MockDateTime.Setup(n => n.UtcNow).Returns(now);

            m_Authentication.ScheduleRefresh(delay);

            Assert.AreEqual(m_Authentication.RefreshActionId, null);
            m_MockScheduler.VerifyAll();
            m_MockDateTime.VerifyAll();
        }

        [Test]
        public void ExecuteScheduledRefresh_ClearsActionId_DoesNothing()
        {
            m_Authentication.RefreshActionId = 1234;
            m_Authentication.ExecuteScheduledRefresh();
            Assert.AreEqual(m_Authentication.RefreshActionId, null);
            Assert.AreEqual(m_Authentication.State, ServerAuthenticationState.Unauthorized);
        }

        [Test]
        public void ExecuteScheduledExpiration_ClearsActionId_Expires()
        {
            m_Authentication.ExpirationActionId = 2345;
            m_Authentication.ExecuteScheduledExpiration();

            Assert.AreEqual(m_Authentication.ExpirationActionId, null);
            Assert.AreEqual(m_Authentication.State, ServerAuthenticationState.Expired);
        }

        [Test]
        public void Expire_CancelsActions_ChangesState()
        {
            var stateChanges = 0;
            ServerAuthenticationState? lastState = null;

            m_Authentication.StateChanged += (ServerAuthenticationState oldState, ServerAuthenticationState newState) =>
            {
                stateChanges++;
                lastState = newState;
            };

            m_Authentication.ExpirationActionId = k_ExpirationActionId;
            m_Authentication.RefreshActionId = k_RefreshActionId;
            m_MockScheduler.Setup(n => n.CancelAction(k_ExpirationActionId));
            m_MockScheduler.Setup(n => n.CancelAction(k_RefreshActionId));
            m_Authentication.Expire();

            Assert.AreEqual(m_Authentication.AccessToken, null);
            Assert.AreEqual(m_AccessTokenComponent.ExpiryTime, null);
            Assert.AreEqual(m_Authentication.ExpirationActionId, null);
            Assert.AreEqual(m_Authentication.State, ServerAuthenticationState.Expired);
            Assert.AreEqual(lastState, ServerAuthenticationState.Expired);
            Assert.AreEqual(stateChanges, 1);

            m_MockScheduler.VerifyAll();
        }

        [Test]
        public void ClearCredentials_ChangesState()
        {
            var stateChanges = 0;

            m_Authentication.State = ServerAuthenticationState.Authorized;

            m_Authentication.StateChanged += (oldState, newState) =>
            {
                stateChanges++;
            };

            m_Authentication.ExpirationActionId = k_ExpirationActionId;
            m_Authentication.RefreshActionId = k_RefreshActionId;
            m_MockScheduler.Setup(n => n.CancelAction(k_ExpirationActionId));
            m_MockScheduler.Setup(n => n.CancelAction(k_RefreshActionId));
            m_Authentication.ClearCredentials();

            Assert.AreEqual(m_Authentication.AccessToken, null);
            Assert.AreEqual(m_AccessTokenComponent.ExpiryTime, null);
            Assert.AreEqual(m_Authentication.ExpirationActionId, null);
            Assert.AreEqual(m_Authentication.State, ServerAuthenticationState.Unauthorized);
            Assert.AreEqual(stateChanges, 1);

            m_MockScheduler.VerifyAll();
        }
    }
}
#endif
