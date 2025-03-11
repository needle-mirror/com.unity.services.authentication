using Unity.Services.Authentication.Server.Internal;

namespace Unity.Services.Authentication.Server
{
    class ServerEnvironmentIdComponent : IServerEnvironmentId
    {
        public string EnvironmentId { get; internal set; }

        internal ServerEnvironmentIdComponent()
        {
        }
    }
}
