using System;
using System.Threading;
using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.Management;
using Nimator.Util;

namespace Nimator.CouchBase
{
    public interface IClusterManagerFactory
    {
        IClusterManager Create();
    }

    public sealed class ClusterManagerFactory : IClusterManagerFactory
    {
        private readonly Lazy<IClusterManager> _initializer;

        public ClusterManagerFactory([NotNull]IAppSettings settings)
            : this(new ClientConfiguration(), settings) { }

        public ClusterManagerFactory([NotNull]ClientConfiguration config, [NotNull]IAppSettings appSettings)
            : this(new Cluster(Guard.AgainstNull_Return(nameof(config), config)), appSettings) { }

        public ClusterManagerFactory([NotNull]string username, [NotNull]string password)
            : this(new ClientConfiguration(), username, password) { }

        public ClusterManagerFactory([NotNull]ClientConfiguration config, [NotNull]string username, [NotNull]string password)
            : this(new Cluster(Guard.AgainstNull_Return(nameof(config), config)), username, password) { }

        public ClusterManagerFactory([NotNull]ICluster cluster, [NotNull]IAppSettings appSettings)
            : this(cluster, Guard.AgainstNull_Return(nameof(appSettings), appSettings).CouchBaseUsername, appSettings.CouchBasePassword) { }

        public ClusterManagerFactory([NotNull]ICluster cluster, [NotNull]string username, [NotNull]string password)
        {
            Guard.AgainstNull(nameof(cluster), cluster);
            Guard.AgainstNullAndEmpty(nameof(username), username);
            Guard.AgainstNullAndEmpty(nameof(password), password);
            
            _initializer = new Lazy<IClusterManager>(() => cluster.CreateManager(username, password), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public static ClusterManagerFactory FromAppSettings(AppSettings settings)
        {
            return new ClusterManagerFactory(settings);
        }

        public IClusterManager Create()
        {
            return _initializer.Value;
        }
    }
}
