using System;
using System.Collections.Generic;
using Couchbase;
using Couchbase.Configuration.Server.Serialization;
using Couchbase.Core;
using Nimator.Util;

namespace Nimator.CouchBase
{
    /// <inheritdoc />
    /// <summary>
    /// Basic implementation of <see cref="T:Nimator.HealthCheck" /> that has DataCollectors for Clusters and Buckets, and no rules.
    /// </summary>
    public class ClusterHealthCheck : HealthCheck
    {
        protected readonly ICollection<ClusterManagerFactory> ClusterManagerFactories = new HashSet<ClusterManagerFactory>();

        public ClusterHealthCheck(params ClusterManagerFactory[] factories)
        {
            Guard.AgainstNull(nameof(factories), factories);
            foreach (var factory in factories)
            {
                Guard.AgainstNull(nameof(factory), factory);
                ClusterManagerFactories.Add(factory);

                AddDataCollector(new DataCollector<IResult<IList<BucketConfig>>>(
                    cacheDuration: TimeSpan.FromSeconds(15),
                    taskFactory: async () => await factory.Value.ListBucketsAsync(),
                    timeout: TimeSpan.FromSeconds(5)));

                AddDataCollector(new DataCollector<IResult<IClusterInfo>>(
                    cacheDuration: TimeSpan.FromSeconds(15),
                    taskFactory: async () => await factory.Value.ClusterInfoAsync(),
                    timeout: TimeSpan.FromSeconds(5)));
            }
        }
    }
}
