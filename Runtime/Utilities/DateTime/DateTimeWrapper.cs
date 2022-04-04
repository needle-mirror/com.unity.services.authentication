using System;

namespace Unity.Services.Authentication
{
    class DateTimeWrapper : IDateTimeWrapper
    {
        static readonly DateTime k_UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public DateTime UtcNow => DateTime.UtcNow;

        public double SecondsSinceUnixEpoch()
        {
            return Math.Round((DateTime.UtcNow - k_UnixEpoch).TotalSeconds);
        }
    }
}
