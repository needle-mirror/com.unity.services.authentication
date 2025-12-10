using Unity.Services.Core.Editor.Environments;
using Unity.Services.Core.Environments.Internal;

namespace Unity.Services.Authentication.Editor
{
    /// <inheritdoc />
    class EnvironmentProvider : IEnvironments
    {
        public string Current => EnvironmentsApi.Instance.ActiveEnvironmentName;
    }
}
