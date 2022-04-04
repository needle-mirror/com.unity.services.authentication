namespace Unity.Services.Authentication
{
    interface ICache
    {
        bool HasKey(string key);
        void DeleteKey(string key);

        void SetString(string key, string value);
        string GetString(string key);
    }
}
