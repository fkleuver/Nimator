using System;
using System.Threading;
using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Management;
using Nimator.Util;

namespace Nimator.CouchBase
{
    public sealed class ClusterManagerFactory : Lazy<IClusterManager>
    {

        public ClusterManagerFactory([NotNull]string username, [NotNull]string password) : this(new ClientConfiguration(), username, password) { }

        public ClusterManagerFactory([NotNull]ClientConfiguration config, [NotNull]string username, [NotNull]string password) : this(
            new Cluster(config), username, password)
        {
            Guard.AgainstNull(nameof(config), config);
        }

        public ClusterManagerFactory([NotNull]ICluster cluster, [NotNull]string username, [NotNull]string password) : base(
            () =>
            {
                var mgr = cluster.CreateManager(username, password);
                return mgr;
            }, LazyThreadSafetyMode.ExecutionAndPublication)
        {
            Guard.AgainstNull(nameof(cluster), cluster);
            Guard.AgainstNullAndEmpty(nameof(username), username);
            Guard.AgainstNullAndEmpty(nameof(password), password);
        }

        public ClusterManagerFactory([NotNull]AppSettings settings) : this(new ClientConfiguration(), settings) { }

        public ClusterManagerFactory([NotNull]ClientConfiguration config, [NotNull]AppSettings settings) : this(
            new Cluster(config), settings)
        {
            Guard.AgainstNull(nameof(config), config);
        }

        public ClusterManagerFactory([NotNull]ICluster cluster, [NotNull]AppSettings settings) : base(
            () =>
            {
                var mgr = cluster.CreateManager(settings.CouchBaseUsername, settings.CouchBasePassword);
                return mgr;
            }, LazyThreadSafetyMode.ExecutionAndPublication)
        {
            Guard.AgainstNull(nameof(cluster), cluster);
            Guard.AgainstNull(nameof(settings), settings);
        }

        public static ClusterManagerFactory FromAppSettings(AppSettings settings)
        {
            return new ClusterManagerFactory(settings);
        }
    }
}
