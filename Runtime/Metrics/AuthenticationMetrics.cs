using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Authentication
{
    class AuthenticationMetrics : IAuthenticationMetrics
    {
        const string k_PackageName = "com.unity.services.authentication";
        const string k_PackageInitTimeKey = "package_init_time";
        const string k_NetworkErrorKey = "network_error_event";
        const string k_ExpiredSessionKey = "expired_session_event";
        const string k_ClientInvalidStateExceptionKey = "client_invalid_state_exception_event";
        const string k_UnlinkExternalIdNotFoundExceptionKey = "unlink_external_id_not_found_exception_event";
        const string k_ClientSessionTokenNotExistsExceptionKey = "client_session_token_not_exists_exception_event";

        readonly IMetrics m_Metrics;

        internal AuthenticationMetrics(IMetricsFactory metricsFactory)
        {
            m_Metrics = metricsFactory.Create(k_PackageName);
        }

        public void SendPackageInitTimeMetric(double initTimeSeconds)
        {
            m_Metrics.SendHistogramMetric(k_PackageInitTimeKey, initTimeSeconds);
        }

        public void SendNetworkErrorMetric()
        {
            m_Metrics.SendSumMetric(k_NetworkErrorKey);
        }

        public void SendExpiredSessionMetric()
        {
            m_Metrics.SendSumMetric(k_ExpiredSessionKey);
        }

        public void SendClientInvalidStateExceptionMetric()
        {
            m_Metrics.SendSumMetric(k_ClientInvalidStateExceptionKey);
        }

        public void SendUnlinkExternalIdNotFoundExceptionMetric()
        {
            m_Metrics.SendSumMetric(k_UnlinkExternalIdNotFoundExceptionKey);
        }

        public void SendClientSessionTokenNotExistsExceptionMetric()
        {
            m_Metrics.SendSumMetric(k_ClientSessionTokenNotExistsExceptionKey);
        }
    }
}
