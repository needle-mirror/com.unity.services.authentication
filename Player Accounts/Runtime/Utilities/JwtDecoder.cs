using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.Services.Authentication.PlayerAccounts
{
    //Temporarily made public for proof of concept sample
    interface IJwtDecoder
    {
        T Decode<T>(string token) where T : BaseJwt;
    }

    /// <summary>
    /// Trimmed-down and specialized version of:
    /// https://github.com/monry/JWT-for-Unity/blob/master/JWT/JWT.cs
    /// At time of writing, this source was public domain (Creative Commons 0)
    /// </summary>
    class JwtDecoder : IJwtDecoder
    {
        static readonly DateTime k_UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        static readonly char[] k_JwtSeparator = { '.' };
        readonly JsonSerializerSettings m_JsonSerializerSettings = new JsonSerializerSettings();

        readonly IDateTimeWrapper m_DateTime;

        internal JwtDecoder(IDateTimeWrapper dateTime)
        {
            m_DateTime = dateTime;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="token"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Decode<T>(string token) where T : BaseJwt
        {
            var parts = token.Split(k_JwtSeparator);
            if (parts.Length == 3)
            {
                var header = parts[0];
                var payload = parts[1];
                var signature = Base64UrlDecode(parts[2]);

                var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(header));
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));

                var headerData = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerJson, m_JsonSerializerSettings);
                var payloadData = JsonConvert.DeserializeObject<T>(payloadJson, m_JsonSerializerSettings);

                // verify exp claim https://tools.ietf.org/html/draft-ietf-oauth-json-web-token-32#section-4.1.4
                var secondsSinceEpoch = m_DateTime.SecondsSinceUnixEpoch();
                if (secondsSinceEpoch >= payloadData.ExpirationTimeUnix)
                {
                    Debug.LogError("Token has expired.");
                    return null;
                }

                return payloadData;
            }

            Debug.LogError($"That is not a valid token (expected 3 parts but has {parts.Length}).");
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding

            var mod4 = input.Length % 4;
            if (mod4 > 0)
            {
                output += new string('=', 4 - mod4);
            }

            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }
    }
}
