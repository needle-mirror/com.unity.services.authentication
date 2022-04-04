using Unity.Services.Core.Configuration.Internal;
using UnityEngine;

namespace Unity.Services.Authentication
{
    class AuthenticationCache : IAuthenticationCache
    {
        public string CloudProjectId => m_CloudProjectId.GetCloudProjectId();
        public string Profile => m_Profile.Current;

        ICloudProjectId m_CloudProjectId;
        IProfile m_Profile;

        string Prefix => $"{CloudProjectId}.{Profile}.unity.services.authentication.";
        string OldPrefix => $"unity.services.authentication.";

        public AuthenticationCache(ICloudProjectId cloudProjectId, IProfile profile)
        {
            m_CloudProjectId = cloudProjectId;
            m_Profile = profile;
        }

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(GetKey(key));
        }

        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(GetKey(key));
        }

        public string GetString(string key)
        {
            return PlayerPrefs.GetString(GetKey(key));
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(GetKey(key), value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// This is to migrate from 'unity.services.authentication' to '{CloudProjectId}.{Profile}.unity.services.authentication'
        /// If the old key exists, we migrate the value to the new key and then delete the old key.
        /// </summary>
        /// <param name="key"></param>
        public void Migrate(string key)
        {
            var oldKey = GetOldKey(key);

            if (PlayerPrefs.HasKey(oldKey))
            {
                var newKey = GetKey(key);
                PlayerPrefs.SetString(newKey, PlayerPrefs.GetString(oldKey));
                PlayerPrefs.DeleteKey(oldKey);
            }
        }

        internal string GetKey(string key)
        {
            return Prefix + key;
        }

        internal string GetOldKey(string key)
        {
            return OldPrefix + key;
        }
    }
}
