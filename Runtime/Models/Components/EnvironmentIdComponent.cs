using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class EnvironmentIdComponent : IEnvironmentId
    {
        public string EnvironmentId { get; internal set; }

        internal EnvironmentIdComponent()
        {
        }
    }
}
