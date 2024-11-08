using NUnit.Framework;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts.Tests
{
    /// <summary>
    /// UriHelperTests
    /// </summary>
    public static class UriHelperTests
    {
        /// <summary>
        /// Tests that UriHelper.ParseQueryString with a valid query works
        /// </summary>
        [Test]
        public static void ParseQueryString_CalledWithValidQueryString_ReturnsCorrectDictionary()
        {
            var queryString = "?key1=value1&key2=value2";

            var result = UriHelper.ParseQueryString(queryString);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["key1"]);
            Assert.AreEqual("value2", result["key2"]);
        }

        /// <summary>
        /// Tests that UriHelper.ParseQueryString with a null throws
        /// </summary>
        [Test]
        public static void ParseQueryString_CalledWithNull_ThrowsException()
        {
            Assert.Throws<PlayerAccountsException>(() => UriHelper.ParseQueryString(null));
        }
    }
}
