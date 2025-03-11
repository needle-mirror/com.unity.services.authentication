using System;
using System.Collections.Generic;

namespace Unity.Services.Authentication.PlayerAccounts
{
    static class UriHelper
    {
        /// <summary>
        /// Parses a URL query string into a dictionary.
        /// </summary>
        /// <param name="queryString">The query string to parse, starting with a '?' or '#' character.</param>
        /// <returns>A dictionary containing the key-value pairs from the query string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when queryString is null.</exception>
        /// <exception cref="ArgumentException">Thrown when queryString does not start with a '?' or '#' character.</exception>
        public static Dictionary<string, string> ParseQueryString(string queryString)
        {
            if (queryString == null)
            {
                throw PlayerAccountsExceptionHandler.HandleError(nameof(queryString), "Query string cannot be null.");
            }

            var dictionary = new Dictionary<string, string>();
            var pairs = queryString.TrimStart('?', '#').Split('&');

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    dictionary[keyValue[0]] = Uri.UnescapeDataString(keyValue[1]);
                }
            }

            return dictionary;
        }
    }
}
