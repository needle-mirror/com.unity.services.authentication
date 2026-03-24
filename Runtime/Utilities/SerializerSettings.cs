using Newtonsoft.Json;

namespace Unity.Services.Authentication
{
    /// <summary>
    /// Json Serialization settings
    /// </summary>
    static class SerializerSettings
    {
        static JsonSerializerSettings s_Instance;

        internal static JsonSerializerSettings DefaultSerializerSettings
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = new JsonSerializerSettings();
                }
                return s_Instance;
            }
        }

        #if UNITY_EDITORs

        [RuntimeInitializeOnLoadMethod]
        private static void ResetStaticsOnLoad()
        {
            s_Instance = null;
        }

        #endif
    }
}
