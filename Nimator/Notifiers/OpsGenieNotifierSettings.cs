using Nimator.Formatters;

namespace Nimator.Notifiers
{
    /// <summary>
    /// Settings for a <see cref="INotifier"/> that calls out to OpsGenie: <see href="https://www.opsgenie.com/">www.opsgenie.com</see>
    /// </summary>
    public sealed class OpsGenieNotifierSettings : NotifierSettings
    {
        /// <summary>
        /// Your API key for posting.
        /// </summary>
        [NotNull]
        public string ApiKey { get; set; }

        /// <summary>
        /// The name of the team to receive new Alerts.
        /// </summary>
        [NotNull]
        public string TeamName { get; set; }

        /// <summary>
        /// The name of the Heartbeat to keep alive on each cycle.
        /// </summary>
        [NotNull]
        public string HeartbeatName { get; set; }

        /// <inheritDoc/>
        public override INotifier ToNotifier()
        {
            return new OpsGenieNotifier(this, new PlainTextFormatter());
        }

        public static NotifierSettings Create()
        {
            return new OpsGenieNotifierSettings();
        }
    }
}
