using UnityEditor;

namespace Unity.Services.Authentication.Editor
{
    class GenesisTokenProvider : IGenesisTokenProvider
    {
        public string Token => CloudProjectSettings.accessToken;
    }
}
