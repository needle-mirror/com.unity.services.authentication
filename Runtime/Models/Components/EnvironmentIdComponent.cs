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
            }
        }

        internal EnvironmentIdComponent()
        {
        }
    }
}
