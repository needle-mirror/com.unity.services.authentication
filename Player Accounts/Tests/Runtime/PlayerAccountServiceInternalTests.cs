#if NUGET_MOQ_AVAILABLE && UNITY_EDITOR
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Core.Configuration.Internal;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.Services.Authentication.PlayerAccounts.Tests
{
    [TestFixture]
    public class PlayerAccountServiceInternalTests
    {
        PlayerAccountServiceInternal m_PlayerAccounts;
        Mock<UnityPlayerAccountSettings> m_MockSettings;
        Mock<IJwtDecoder> m_MockJwtDecoder;
        Mock<ICloudProjectId> m_MockCloudProjectId;
        Mock<INetworkHandler> m_MockNetwork;

        [SetUp]
        public void Setup()
        {
            m_MockSettings = new Mock<UnityPlayerAccountSettings>();
            m_MockJwtDecoder = new Mock<IJwtDecoder>();
            m_MockNetwork = new Mock<INetworkHandler>();
            m_MockCloudProjectId = new Mock<ICloudProjectId>();
            m_PlayerAccounts = new PlayerAccountServiceInternal(m_MockSettings.Object, m_MockCloudProjectId.Object, m_MockJwtDecoder.Object, m_MockNetwork.Object);
        }

        [UnityTest]
        public IEnumerator SignIn_Authorized_ShouldNotSignIn() => AsCoroutine(SignInAsync_Authorized_ShouldNotSignInAsync);

        async Task SignInAsync_Authorized_ShouldNotSignInAsync()
        {
            // Arrange
            m_PlayerAccounts.SignInState = PlayerAccountState.Authorized;

            // Act
            Exception exception = null;
            try
            {
                await m_PlayerAccounts.StartSignInAsync();
            }
            catch (PlayerAccountsException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<PlayerAccountsException>(exception);
            Assert.AreEqual(PlayerAccountsErrorCodes.InvalidState, ((PlayerAccountsException)exception).ErrorCode);
            Assert.AreEqual("Player is already signed in.", exception.Message);
            m_MockJwtDecoder.VerifyAll();
        }

        [UnityTest]
        public IEnumerator SignIn_Refreshing_ShouldNotSignIn() => AsCoroutine(SignInAsync_Refreshing_ShouldNotSignInAsync);

        async Task SignInAsync_Refreshing_ShouldNotSignInAsync()
        {
            // Arrange
            m_PlayerAccounts.SignInState = PlayerAccountState.Refreshing;

            // Act
            Exception exception = null;
            try
            {
                await m_PlayerAccounts.StartSignInAsync();
            }
            catch (PlayerAccountsException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<PlayerAccountsException>(exception);
            Assert.AreEqual(PlayerAccountsErrorCodes.InvalidState, ((PlayerAccountsException)exception).ErrorCode);
            Assert.AreEqual("Player is already signed in.", exception.Message);
            m_MockJwtDecoder.VerifyAll();
        }

        [UnityTest]
        public IEnumerator RefreshToken_NotSignedIn_ThrowsException() => AsCoroutine(RefreshTokenAsync_NotSignedIn_ThrowsException);

        async Task RefreshTokenAsync_NotSignedIn_ThrowsException()
        {
            // Arrange
            m_PlayerAccounts.SignInState = PlayerAccountState.SignedOut;

            // Act
            try
            {
                await m_PlayerAccounts.RefreshTokenAsync();
                Assert.Fail("An exception was expected to be thrown.");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Player is not signed in.", ex.Message);
            }

            // Assert
            Assert.AreEqual(PlayerAccountState.SignedOut, m_PlayerAccounts.SignInState);
        }

        [Test]
        public void SignOut_ClearsAccessToken()
        {
            // Arrange
            m_PlayerAccounts.SignInState = PlayerAccountState.Authorized;

            // Act
            m_PlayerAccounts.SignOut();

            // Assert
            Assert.IsNull(m_PlayerAccounts.AccessToken);
            Assert.AreEqual(PlayerAccountState.SignedOut, m_PlayerAccounts.SignInState);
        }

        static IEnumerator AsCoroutine(Func<Task> test)
        {
            var task = test();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                if (task.Exception.InnerException != null)
                {
                    throw task.Exception.InnerException;
                }
                throw task.Exception;
            }
        }
    }
}
#endif
