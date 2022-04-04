namespace Unity.Services.Authentication
{
    interface IAuthenticationCache : ICache
    {
        string Profile { get; }
        string CloudProjectId { get; }

        void Migrate(string key);
    }
}
