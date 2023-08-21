using Unity.Services.Core.Configuration.Internal;
using UnityEngine;

namespace Unity.Services.Authentication.Editor
{
    /// <inheritdoc />
    class CloudProjectIdProvider : ICloudProjectId
    {
        public string GetCloudProjectId()
        {
            return Application.cloudProjectId;
        }
    }
}
