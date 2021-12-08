using System.Runtime.CompilerServices;


namespace Unity.Services.Authentication.Editor
{
    interface IGenesisTokenProvider
    {
        string Token { get; }
    }
}
