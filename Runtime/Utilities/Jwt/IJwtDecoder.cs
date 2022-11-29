namespace Unity.Services.Authentication
{
    interface IJwtDecoder
    {
        T Decode<T>(string token) where T : BaseJwt;
    }
}
