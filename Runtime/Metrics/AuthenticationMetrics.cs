using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Authentication
{
    class AuthenticationMetrics : IAuthenticationMetrics
    {
        const string AuthenticationPackageName = "com.unity.services.authentication";
        const string NetworkErrorMetricName = "network_error_event";
        const string ExpiredSessionMetricName = "expired_session_event";
        const string ClientInvalidStateExceptionMetricName = "client_invalid_state_exception_event";
        const string UnlinkExternalIdNotFoundExceptionMetricName = "unlink_external_id_not_found_exception_event";
        const string ClientSessionTokenNotExistsExceptionMetricName = "client_session_token_not_exists_exception_event";

        readonly IMetrics m_Metrics;

        internal AuthenticationMetrics(IMetricsFactory metricsFactory)
        {
            m_Metrics = metricsFactory.Create(AuthenticationPackageName);
        }

        public void SendNetworkErrorMetric()
        {
            m_Metrics.SendSumMetric(NetworkErrorMetricName);
        }

        public void SendExpiredSessionMetric()
        {
            m_Metrics.SendSumMetric(ExpiredSessionMetricName);
        }

        public void SendClientInvalidStateExceptionMetric()
        {
            m_Metrics.SendSumMetric(ClientInvalidStateExceptionMetricName);
        }

        public void SendUnlinkExternalIdNotFoundExceptionMetric()
        {
            m_Metrics.SendSumMetric(UnlinkExternalIdNotFoundExceptionMetricName);
        }

        public void SendClientSessionTokenNotExistsExceptionMetric()
        {
            m_Metrics.SendSumMetric(ClientSessionTokenNotExistsExceptionMetricName);
        }
    }
}
