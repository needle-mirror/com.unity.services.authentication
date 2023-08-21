using System.Collections.Generic;
using Unity.Services.Core.Telemetry.Internal;

namespace Unity.Services.Authentication.Editor
{
    /// <inheritdoc />
    class DisabledMetricsFactory : IMetricsFactory
    {
        IReadOnlyDictionary<string, string> IMetricsFactory.CommonTags { get; }
            = new Dictionary<string, string>();

        IMetrics IMetricsFactory.Create(string packageName) => new DisabledMetrics();

        class DisabledMetrics : IMetrics
        {
            void IMetrics.SendGaugeMetric(string name, double value, IDictionary<string, string> tags)
            {
                // Do nothing since it's disabled.
            }

            void IMetrics.SendHistogramMetric(string name, double time, IDictionary<string, string> tags)
            {
                // Do nothing since it's disabled.
            }

            void IMetrics.SendSumMetric(string name, double value, IDictionary<string, string> tags)
            {
                // Do nothing since it's disabled.
            }
        }
    }
}
