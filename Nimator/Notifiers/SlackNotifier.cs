using System;
using Nimator.Formatters;
using Nimator.Util;

namespace Nimator.Notifiers
{
    public sealed class SlackNotifier : INotifier
    {
        private readonly SlackNotifierSettings _settings;
        private readonly IHealthCheckResultFormatter _formatter;
        private DateTime _dontAlertBefore;

        public SlackNotifier(
            [NotNull]SlackNotifierSettings settings,
            [CanBeNull]IHealthCheckResultFormatter formatter = null)
        {
            Guard.AgainstNull(nameof(settings), settings);
            Guard.AgainstNullAndEmpty(nameof(settings.Url), settings.Url);

            _settings = settings;
            _formatter = formatter ?? new PlainTextFormatter();
        }
        
        public void Send([NotNull]HealthCheckResult result)
        {
            Guard.AgainstNull(nameof(result), result);

            if (_settings.DebounceTimeInSecs > 0 && DateTime.Now < _dontAlertBefore)
            {
                return;
            }
            
            result.Finalize(result.CheckId, r => r.Level >= _settings.Threshold);
            if (result.Level >= _settings.Threshold)
            {
                var message = new SlackMessage(result, _formatter);
                
                if (_settings.DebounceTimeInSecs > 0){
                    _dontAlertBefore = DateTime.Now.AddSeconds(_settings.DebounceTimeInSecs);
                    message.AddAttachment("Debouncing messages until at least *" + _dontAlertBefore.ToString("yyyy-MM-dd, HH:mm:ss") + "*, even if more problems arise.");
                }
                    
                SimpleRestUtils.PostToRestApi(_settings.Url, message);
            }
        }
    }
}
