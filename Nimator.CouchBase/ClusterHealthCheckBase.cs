using System;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Management;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.CouchBase
{
    public abstract class ClusterHealthCheckBase : HealthCheckBase
    {
        protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(10);
        private readonly IClusterManagerFactory _factory;

        protected ClusterHealthCheckBase([NotNull]IClusterManagerFactory factory)
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
                var data = await manager.ClusterInfoAsync();
                if (!data.Success)
                {
                    health = HealthCheckResult.Create(Id)
                        .SetStatus(Status.Critical)
                        .SetLevel(LogLevel.Error)
                        .SetReason($"CouchBase Cluster failed to retrieve cluster info.");
                }
                else
                {
                    health = await GetHealthCheckResult(data.Value);
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

        protected abstract Task<HealthCheckResult> GetHealthCheckResult(IClusterInfo cluster);
    }
}
