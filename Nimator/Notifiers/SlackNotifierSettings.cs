using Nimator.Formatters;

namespace Nimator.Notifiers
{
    /// <summary>
    /// Settings for creating a <see cref="INotifier"/> that will publish to Slack: <see href="https://slack.com/">slack.com</see>
    /// </summary>
    public sealed class SlackNotifierSettings : NotifierSettings
    {
        /// <summary>
        /// The webhook integration url to post to.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// A number of seconds to wait for any subsequent post.
        /// </summary>
        public int DebounceTimeInSecs { get; set; }

        /// <inheritDoc/>
        public override INotifier ToNotifier()
        {
            return new SlackNotifier(this, new PlainTextFormatter());
        }

        public static NotifierSettings Create()
        {
            return new SlackNotifierSettings();
        }
    }
}
