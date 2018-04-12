using System;
using System.Threading.Tasks;
using Couchbase.Configuration.Server.Serialization;
using Couchbase.Management;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.CouchBase
{
    public abstract class BucketsHealthCheckBase : HealthCheckBase
    {
        protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(10);
        private readonly IClusterManagerFactory _factory;

        protected BucketsHealthCheckBase([NotNull]IClusterManagerFactory factory)
        {
            _factory = factory;
            Guard.AgainstNull(nameof(factory), factory);
        }
        
        protected override async Task<HealthCheckResult> GetHealthCheckResult()
        {
            IClusterManager manager;
            try
            {
                manager = _factory.Create();
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);

                return HealthCheckResult.Create(Id)
                    .SetStatus(Status.Unknown)
                    .SetLevel(LogLevel.Fatal)
                    .SetReason($"HealthCheck {Id} failed to connect to CouchBase.")
                    .SetException(e);
            }

            HealthCheckResult health;
            try
            {
                var data = await manager.ListBucketsAsync();
                if (!data.Success)
                {
                    health = HealthCheckResult.Create(Id)
                        .SetStatus(Status.Critical)
                        .SetLevel(LogLevel.Error)
                        .SetReason($"CouchBase Cluster failed to retrieve bucket configurations.");
                }
                else
                {
                    health = HealthCheckResult.Create(Id);
                    foreach (var bucket in data.Value)
                    {
                        var bucketHealth = await GetHealthCheckResult(bucket);
                        health.AddInnerResult(bucketHealth);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorException(e.Message, e);

                health = HealthCheckResult.Create(Id)
                    .SetStatus(Status.Unknown)
                    .SetLevel(LogLevel.Fatal)
                    .SetReason($"HealthCheck {Id} threw an exception.")
                    .SetException(e);
            }

            return health;
        }

        protected abstract Task<HealthCheckResult> GetHealthCheckResult(IBucketConfig bucket);
    }
}
