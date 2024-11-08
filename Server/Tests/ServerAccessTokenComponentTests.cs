#if NUGET_MOQ_AVAILABLE && UNITY_EDITOR
using NUnit.Framework;
using System;

namespace Unity.Services.Authentication.Server.Tests
{
    [TestFixture]
    public class ServerAccessTokenComponentTests
    {
        const string k_TokenValue = "t0k3n";

        ServerAccessTokenComponent m_Component;

        [SetUp]
        public void SetUp()
        {
            m_Component = new ServerAccessTokenComponent();
        }

        [Test]
        public void SetterDoesNotTrigger()
        {
            m_Component.AccessToken = k_TokenValue;

            m_Component.AccessTokenChanged += (token) =>
            {
                Assert.Fail("This should not be called");
            };

            m_Component.AccessToken = k_TokenValue;
        }

        [Test]
        public void SetterTrigger()
        {
            var eventCount = 0;

            m_Component.AccessTokenChanged += (token) =>
            {
                eventCount++;
                Assert.AreEqual(k_TokenValue, token);
            };

            m_Component.AccessToken = k_TokenValue;
            Assert.AreEqual(eventCount, 1);
        }

        [Test]
        public void Clear()
        {
            m_Component.AccessToken = k_TokenValue;
            m_Component.ExpiryTime = DateTime.MaxValue;

            Assert.AreEqual(m_Component.AccessToken, k_TokenValue);
            Assert.AreEqual(m_Component.ExpiryTime.Value, DateTime.MaxValue);

            m_Component.Clear();

            Assert.IsNull(m_Component.AccessToken);
            Assert.IsFalse(m_Component.ExpiryTime.HasValue);
        }
    }
}
#endif
