using System;
using Nimator.Formatters;
using Nimator.Util;

namespace Nimator.Notifiers
{
    public sealed class ConsoleNotifier : INotifier
    {
        private readonly ConsoleNotifierSettings _settings;
        private readonly IHealthCheckResultFormatter _formatter;
        private readonly Action<string> _writeLine;

        public ConsoleNotifier(
            [NotNull]ConsoleNotifierSettings settings,
            [CanBeNull]IHealthCheckResultFormatter formatter = null,
            [CanBeNull]Action<string> writeLine = null)
        {
            Guard.AgainstNull(nameof(settings), settings);

            _settings = settings;
            _formatter = formatter ?? new JsonHealthCheckResultFormatter();
            _writeLine = writeLine ?? Console.WriteLine;
        }

        public void Send([NotNull]HealthCheckResult result)
        {
            Guard.AgainstNull(nameof(result), result);

            result.Finalize(result.CheckId, r => r.Level >= _settings.Threshold);
            if (result.Level >= _settings.Threshold)
            {
                _writeLine(_formatter.Format(result));
            }
        }
    }
}
