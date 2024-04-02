#if NUGET_MOQ_AVAILABLE && UNITY_EDITOR
using System;
using Moq;
using NUnit.Framework;
using Unity.Services.Core;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.Tests
{
    class PlayerAccountServiceOverride : IDisposable
    {
        IPlayerAccountService m_OldValue;

        public PlayerAccountServiceOverride()
        {
            try
            {
                m_OldValue = PlayerAccountService.Instance;
            }
            catch (Exception)
            {
                // leave it null and ignore whatever exception.
            }
        }

        public void Dispose()
        {
            PlayerAccountService.Instance = m_OldValue;
        }
    }

    [TestFixture]
    class PlayerAccountServiceTests
    {
        Mock<IPlayerAccountService> m_MockPlayerAccounts = new Mock<IPlayerAccountService>();

        [Test]
        public void TestSingletonInitialized()
        {
            using (new PlayerAccountServiceOverride())
            {
                PlayerAccountService.Instance = m_MockPlayerAccounts.Object;

                var x = PlayerAccountService.Instance;
                Assert.IsNotNull(x);
            }
        }

        [Test]
        public void TestSingletonUninitialized()
        {
            using (new PlayerAccountServiceOverride())
            {
                PlayerAccountService.Instance = null;
                try
                {
                    var _ = PlayerAccountService.Instance;
                    Assert.Fail();
                }
                catch (ServicesInitializationException e)
                {
                    Assert.AreEqual("Singleton is not initialized. " +
                        "Please call UnityServices.InitializeAsync() to initialize. " +
                        "Please make sure Player Accounts is configured in the Unity Editor Settings", e.Message);
                }
            }
        }
    }
}
#endif
