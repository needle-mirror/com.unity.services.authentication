using System;
using Unity.Services.Authentication.Internal;

namespace Unity.Services.Authentication
{
    class AccessTokenComponent : IAccessToken
    {
        public string AccessToken { get; internal set; }
        public DateTime? ExpiryTime { get; internal set; }

        internal AccessTokenComponent()
        {
        }

        internal void Clear()
        {
            AccessToken = null;
            ExpiryTime = null;
        }
    }
}
