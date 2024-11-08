using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class EnvironmentIdComponent : IEnvironmentId
    {
        string m_EnvironmentId;

        public string EnvironmentId
        {
            get => m_EnvironmentId;
            internal set
            {
                m_EnvironmentId = value;

#if ENABLE_CLOUD_SERVICES_IDENTIFIERS
                UnityEngine.Connect.Identifiers.SetEnvironmentId(value);
#endif
            }
        }

        internal EnvironmentIdComponent()
        {
        }
    }
}
