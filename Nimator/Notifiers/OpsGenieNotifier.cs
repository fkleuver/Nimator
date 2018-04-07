using System.Linq;
using Nimator.Formatters;
using Nimator.Util;
using LogLevel = Nimator.Logging.LogLevel;

namespace Nimator.Notifiers
{
    public sealed class OpsGenieNotifier : INotifier
    {
        private const int MaxOpsgenieTagLength = 50;
        private const int MaxOpsgenieMessageLength = 130;
        private const string AlertUrl = "https://api.opsgenie.com/v1/json/alert";
        private const string HeartbeatUrl = "https://api.opsgenie.com/v1/json/heartbeat/send";
        private readonly OpsGenieNotifierSettings _settings;
        private readonly IHealthCheckResultFormatter _formatter;

        public OpsGenieNotifier(
            [NotNull]OpsGenieNotifierSettings settings,
            [CanBeNull]IHealthCheckResultFormatter formatter)
        {
            Guard.AgainstNull(nameof(settings), settings);
            Guard.AgainstNullAndEmpty(nameof(settings.ApiKey), settings.ApiKey);
            Guard.AgainstNullAndEmpty(nameof(settings.HeartbeatName), settings.HeartbeatName);
            Guard.AgainstNullAndEmpty(nameof(settings.TeamName), settings.TeamName);

            _settings = settings;
            _formatter = formatter ?? new PlainTextFormatter();
        }
        
        public void Send([NotNull]HealthCheckResult result)
        {
            Guard.AgainstNull(nameof(result), result);

            SendHeartbeat();
            
            result.Finalize(result.CheckId, r => r.Level >= _settings.Threshold);
            if (result.Level >= _settings.Threshold)
            {
                NotifyFailureResult(result);
            }
        }

        private void SendHeartbeat()
        {
            var request = new OpsGenieHeartbeatRequest(this._settings.ApiKey, this._settings.HeartbeatName);
            SimpleRestUtils.PostToRestApi(HeartbeatUrl, request);
        }

        private void NotifyFailureResult(HealthCheckResult result)
        {
            var failingLayerName = (result.AllResults.FirstOrDefault(r => r.Level >= LogLevel.Error)?.CheckId?.Name ?? "UnknownLayer").Truncate(MaxOpsgenieTagLength);
            var message = result.Reason.Truncate(MaxOpsgenieMessageLength);

            var request = new OpsGenieCreateAlertRequest(this._settings.ApiKey, message)
            {
                Alias = "nimator-failure",
                Description = _formatter.Format(result),
                Teams = new[] { this._settings.TeamName },
                Tags = new[] { "Nimator", failingLayerName }
            };

            SimpleRestUtils.PostToRestApi(AlertUrl, request);
        }
    }
}
