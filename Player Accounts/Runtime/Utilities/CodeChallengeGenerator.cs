using System;
using System.Security.Cryptography;
using System.Text;

namespace Unity.Services.Authentication.PlayerAccounts
{
    class CodeChallengeGenerator
    {
        const int k_CodeLength = 128;

        const string k_CodeChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        readonly StringBuilder m_CodeBuilder;

        internal CodeChallengeGenerator()
        {
            m_CodeBuilder = new StringBuilder(k_CodeLength);
        }

        public string GenerateCode()
        {
            var randomBytes = new byte[k_CodeLength];
            using (var randomNumberGenerator = new RNGCryptoServiceProvider())
            {
                randomNumberGenerator.GetBytes(randomBytes);
            }

            m_CodeBuilder.Clear();
            for (var i = 0; i < k_CodeLength; i++)
            {
                m_CodeBuilder.Append(k_CodeChars[randomBytes[i] % k_CodeChars.Length]);
            }

            return m_CodeBuilder.ToString();
        }

        public string GenerateStateString()
        {
            return Guid.NewGuid().ToString();
        }

        public static string S256EncodeChallenge(string code)
        {
            var sha256 = SHA256.Create();
            var codeVerifierBytes = Encoding.UTF8.GetBytes(code);
            var codeVerifierHash = sha256.ComputeHash(codeVerifierBytes);
            return UrlSafeBase64Encode(codeVerifierHash);
        }

        static string UrlSafeBase64Encode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }
    }
}
