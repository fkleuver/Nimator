using Nimator.Util;

namespace Nimator.Web.Hubs
{
    public sealed class SignalRNotifier : INotifier
    {
        private readonly SignalRNotifierSettings _settings;
        private readonly HealthCheckResultsBroadcaster _broadcaster;

        public SignalRNotifier(
            [NotNull]SignalRNotifierSettings settings,
            [NotNull]HealthCheckResultsBroadcaster broadcaster = null)
        {
            Guard.AgainstNull(nameof(settings), settings);

            _settings = settings;
            _broadcaster = broadcaster ?? HealthCheckResultsBroadcaster.Instance;
        }

        public void Send(HealthCheckResult result)
        {
            result.Finalize(result.CheckId, r => r.Level >= _settings.Threshold);
            _broadcaster.HealthCheckResultNotify(result).Wait();
        }
    }
}
