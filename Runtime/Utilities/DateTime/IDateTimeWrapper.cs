using System;

namespace Unity.Services.Authentication
{
    interface IDateTimeWrapper
    {
        double SecondsSinceUnixEpoch();

        DateTime UtcNow { get; }
    }
}
