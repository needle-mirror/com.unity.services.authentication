using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Trimmed-down and specialized version of:
    /// https://github.com/monry/JWT-for-Unity/blob/master/JWT/JWT.cs
    /// At time of writing, this source was public domain (Creative Commons 0)
    /// </summary>
    class JwtDecoder : IJwtDecoder
    {
        static readonly char[] k_JwtSeparator = { '.' };

        public T Decode<T>(string token) where T : BaseJwt
        {
            var parts = token.Split(k_JwtSeparator);
            if (parts.Length == 3)
            {
                var payload = parts[1];
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
                var payloadData = IsolatedJsonConvert.DeserializeObject<T>(payloadJson, SerializerSettings.DefaultSerializerSettings);

                return payloadData;
            }

            Logger.LogError($"That is not a valid token (expected 3 parts but has {parts.Length}).");
            return null;
        }

        byte[] Base64UrlDecode(string input)
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
