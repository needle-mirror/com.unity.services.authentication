using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[assembly: AlwaysLinkAssembly]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Editor")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Editor.Shared")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Internal1")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Internal2")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Internal3")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Internal4")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Unity")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Tests")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.EditorTests")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.TestUtils")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // For Moq

#if UNITY_SERVER || ENABLE_UCS_SERVER
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Server")]
[assembly: InternalsVisibleTo("Unity.Services.Authentication.Server.Tests")]
#endif
