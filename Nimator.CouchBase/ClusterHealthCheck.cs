using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase;
using Couchbase.Configuration.Server.Serialization;
using Couchbase.Core;
using Nimator.CouchBase.Rules;
using Nimator.Util;

namespace Nimator.CouchBase
{
    /// <inheritdoc />
    /// <summary>
    /// Basic implementation of <see cref="T:Nimator.HealthCheck" /> that has DataCollectors for Clusters and Buckets, and no rules.
    /// </summary>
    public class ClusterHealthCheck : HealthCheck
    {
        protected readonly ICollection<IClusterManagerFactory> ClusterManagerFactories = new HashSet<IClusterManagerFactory>();
        
        public ClusterHealthCheck([NotNull]IEnumerable<IClusterManagerFactory> factories)
            : this (Guard.AgainstNull_Return(nameof(factories), factories).ToArray()) { }

        public ClusterHealthCheck([NotNull, ItemNotNull, NotEmpty]params IClusterManagerFactory[] factories)
        {
            Guard.AgainstNullAndEmpty(nameof(factories), factories);
            foreach (var factory in factories)
            {
                Guard.AgainstNull(nameof(factory), factory);
                ClusterManagerFactories.Add(factory);

                AddDataCollector(new DataCollector<IResult<IList<BucketConfig>>>(
                    cacheDuration: TimeSpan.FromSeconds(15),
                    taskFactory: async () => await factory.Create().ListBucketsAsync(),
                    timeout: TimeSpan.FromSeconds(5)));

                AddDataCollector(new DataCollector<IResult<IClusterInfo>>(
                    cacheDuration: TimeSpan.FromSeconds(15),
                    taskFactory: async () => await factory.Create().ClusterInfoAsync(),
                    timeout: TimeSpan.FromSeconds(5)));
            }

            AddRule(new ServiceUnresponsive(Id));
        }
    }
}
