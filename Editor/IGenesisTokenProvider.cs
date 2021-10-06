using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.Services.Authentication.Editor.Tests")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.EditorTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // For Moq

namespace Unity.Services.Authentication.Editor
{
    interface IGenesisTokenProvider
    {
        string Token { get; }
    }
}
