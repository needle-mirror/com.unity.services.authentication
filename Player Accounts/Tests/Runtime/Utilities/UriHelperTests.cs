using NUnit.Framework;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.Tests
{
    public static class UriHelperTests
    {
        [Test]
        public static void ParseQueryString_CalledWithValidQueryString_ReturnsCorrectDictionary()
        {
            var queryString = "?key1=value1&key2=value2";

            var result = UriHelper.ParseQueryString(queryString);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["key1"]);
            Assert.AreEqual("value2", result["key2"]);
        }

        [Test]
        public static void ParseQueryString_CalledWithNull_ThrowsException()
        {
            Assert.Throws<PlayerAccountsException>(() => UriHelper.ParseQueryString(null));
        }
    }
}
