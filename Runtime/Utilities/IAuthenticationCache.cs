namespace Unity.Services.Authentication.Utilities
{
    interface IAuthenticationCache : ICache
    {
        string Profile { get; }
        string CloudProjectId { get; }

        void Migrate(string key);
    }
}
