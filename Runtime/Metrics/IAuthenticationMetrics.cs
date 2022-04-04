using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Authentication
{
    interface IAuthenticationMetrics
    {
        void SendNetworkErrorMetric();

        void SendExpiredSessionMetric();

        void SendClientInvalidStateExceptionMetric();

        void SendUnlinkExternalIdNotFoundExceptionMetric();

        void SendClientSessionTokenNotExistsExceptionMetric();
    }
}
