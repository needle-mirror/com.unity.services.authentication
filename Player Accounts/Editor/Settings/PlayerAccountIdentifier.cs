using Unity.Services.Core.Editor;

namespace Unity.Services.Authentication.PlayerAccounts.Editor
{
    /// <summary>
    /// Implementation of the <see cref="IEditorGameServiceIdentifier"/> for the Player Accounts package
    /// </summary>
    /// <remarks>This identifier MUST be public struct.</remarks>
    struct PlayerAccountIdentifier : IEditorGameServiceIdentifier
    {
        /// <summary>
        /// Key for the Player Accounts package
        /// </summary>
        /// <returns>The identifier of the game service.</returns>
        public string GetKey() => "Unity Player Accounts";
    }
}
