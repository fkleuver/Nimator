using Nimator.Formatters;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.Notifiers
{
    public sealed class LibLogNotifier : INotifier
    {
        private readonly LibLogNotifierSettings _settings;
        private readonly ILog _logger;
        private readonly IHealthCheckResultFormatter _formatter;
        
        public LibLogNotifier(
            [NotNull]LibLogNotifierSettings settings,
            [CanBeNull]ILog logger = null,
            [CanBeNull]IHealthCheckResultFormatter formatter = null)
        {
            Guard.AgainstNull(nameof(settings), settings);

            _settings = settings;
            _logger = logger ?? LogProvider.GetCurrentClassLogger();
            _formatter = formatter ?? new JsonHealthCheckResultFormatter();
        }
        
        public void Send([NotNull]HealthCheckResult result)
        {
            Guard.AgainstNull(nameof(result), result);

            if (result.Level >= _settings.Threshold)
            {
                result.Finalize(result.CheckId, r => r.Level >= _settings.Threshold);
                _logger.Log(result.Level, () => _formatter.Format(result));
            }
        }
    }
}
