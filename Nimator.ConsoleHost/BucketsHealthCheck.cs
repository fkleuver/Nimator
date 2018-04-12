using System.Threading.Tasks;
using Couchbase.Configuration.Server.Serialization;
using Nimator.CouchBase;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.ConsoleHost
{
    public sealed class BucketsHealthCheck : BucketsHealthCheckBase
    {
        public BucketsHealthCheck() : base(ClusterManagerFactory.FromAppSettings(AppSettings.FromConfigurationManager())) { }

        protected override Task<HealthCheckResult> GetHealthCheckResult(IBucketConfig bucket)
        {
            var health = HealthCheckResult.Create(bucket.Name);
            if (bucket.BasicStats.ItemCount > 100000)
            {
                health.SetStatus(Status.Warning).SetLevel(LogLevel.Warn).SetReason($"Bucket {bucket.Name} has more than 100000 documents.");
            }
            else
            {
                health.SetStatus(Status.Okay);
            }

            return Task.FromResult(health);
        }
    }
}
