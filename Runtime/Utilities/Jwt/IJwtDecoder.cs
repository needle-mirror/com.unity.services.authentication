namespace Unity.Services.Authentication
{
    interface IJwtDecoder
    {
        T Decode<T>(string token, WellKnownKey[] keys) where T : BaseJwt;
    }
}
