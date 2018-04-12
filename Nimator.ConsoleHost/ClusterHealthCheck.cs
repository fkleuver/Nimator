using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Nimator.CouchBase;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.ConsoleHost
{
    public sealed class ClusterHealthCheck : ClusterHealthCheckBase
    {
        public ClusterHealthCheck() : base(ClusterManagerFactory.FromAppSettings(AppSettings.FromConfigurationManager())) { }

        protected override Task<HealthCheckResult> GetHealthCheckResult(IClusterInfo cluster)
        {
            var health = HealthCheckResult.Create(Id);
            var ram = cluster.Pools().StorageTotals.Ram;
            var used = (int)Math.Round((double)ram.QuotaUsed / ram.QuotaTotal * 100, 0);
            if (used > 85)
            {
                health.SetStatus(Status.Warning).SetLevel(LogLevel.Warn).SetReason($"Available quota memory is less than 15%.");
            }
            else
            {
                health.SetStatus(Status.Okay);
            }

            return Task.FromResult(health);
        }
    }
}
