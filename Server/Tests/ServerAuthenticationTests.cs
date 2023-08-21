#if NUGET_MOQ_AVAILABLE && UNITY_EDITOR
using Moq;
using NUnit.Framework;
using System;
using Unity.Services.Core;

namespace Unity.Services.Authentication.Server.Tests
{
    /// <summary>
    /// The helper class that keeps track of the singleton and
    /// reset it after the test case is done.
    /// </summary>
    class ServerAuthenticationOverride : IDisposable
    {
        IServerAuthenticationService m_OldValue;

        public ServerAuthenticationOverride()
        {
            try
            {
                m_OldValue = ServerAuthenticationService.Instance;
            }
            catch (Exception)
            {
                // leave it null and ignore whatever exception.
            }
        }

        public void Dispose()
        {
            ServerAuthenticationService.Instance = m_OldValue;
        }
    }

    [TestFixture]
    public class ServerAuthenticationTests
    {
        Mock<IServerAuthenticationService> m_MockAuthentication = new Mock<IServerAuthenticationService>();

        [Test]
        public void TestSingletonInitialized()
        {
            using (new ServerAuthenticationOverride())
            {
                ServerAuthenticationService.Instance = m_MockAuthentication.Object;

                var x = ServerAuthenticationService.Instance;
                Assert.IsNotNull(x);
            }
        }

        [Test]
        public void TestSingletonUninitialized()
        {
            using (new ServerAuthenticationOverride())
            {
                ServerAuthenticationService.Instance = null;
                try
                {
                    var _ = ServerAuthenticationService.Instance;
                    Assert.Fail();
                }
                catch (ServicesInitializationException e)
                {
                    Assert.AreEqual("Singleton is not initialized. " +
                        "Please call UnityServices.InitializeAsync() to initialize.", e.Message);
                }
            }
        }
    }
}
#endif
