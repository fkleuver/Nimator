using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Nimator.Web.Hubs
{
    /// <summary>
    /// SignalR event hub for pushing health check results to the client
    /// </summary>
    [HubName("healthCheckResultsHub")]
    public sealed class HealthCheckResultsHub : Hub
    {
        private readonly HealthCheckResultsBroadcaster _healthCheckResultsBroadcaster;
        public HealthCheckResultsHub() : this(HealthCheckResultsBroadcaster.Instance) { }

        public HealthCheckResultsHub(HealthCheckResultsBroadcaster broadcaster)
        {
            _healthCheckResultsBroadcaster = broadcaster;
        }
    }

    public sealed class HealthCheckResultsBroadcaster
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<HealthCheckResultsBroadcaster> _instance = new Lazy<HealthCheckResultsBroadcaster>(() =>
            new HealthCheckResultsBroadcaster(GlobalHost.ConnectionManager.GetHubContext<HealthCheckResultsHub>().Clients), true);

        public static HealthCheckResultsBroadcaster Instance => _instance.Value;

        private IHubConnectionContext<dynamic> Clients { get; set; }

        internal HealthCheckResultsBroadcaster(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        internal async Task HealthCheckResultNotify(HealthCheckResult result)
        {
            await Clients.All.healthCheckResult(result);
        }
    }
}
