using Nimator.Logging;
using Nimator.Notifiers;

namespace Nimator.Web.Hubs
{
    /// <summary>
    /// Settings for a <see cref="T:Nimator.Notifiers.SignalRNotifier" />.
    /// </summary>
    public sealed class SignalRNotifierSettings : NotifierSettings
    {
        /// <summary>
        /// Constructs default settings
        /// </summary>
        public SignalRNotifierSettings()
        {
            Threshold = LogLevel.Info;
        }

        /// <inheritDoc/>
        public override INotifier ToNotifier()
        {
            return new SignalRNotifier(this, HealthCheckResultsBroadcaster.Instance);
        }

        public static NotifierSettings Create()
        {
            return new SignalRNotifierSettings();
        }
    }
}
