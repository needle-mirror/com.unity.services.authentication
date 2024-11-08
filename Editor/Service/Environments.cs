using Unity.Services.Core.Environments.Internal;

namespace Unity.Services.Authentication.Editor
{
    /// <inheritdoc />
    class EnvironmentProvider : IEnvironments
    {
#if UNITY_2020_OR_NEWER
        public string Current => EnvironmentsApi.Instance.ActiveEnvironmentName;
#else
        // Not supported
        public string Current => null;
#endif
    }
}
