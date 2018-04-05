using Nimator.Logging;

namespace Nimator.Notifiers
{
    /// <summary>
    /// Abstract structure for settings to bootstrap <see cref="INotifier"/> instances.
    /// </summary>
    public abstract class NotifierSettings
    {
        /// <summary>
        /// Level threshold at which notifications should start to be sent out by this notifier.
        /// </summary>
        public LogLevel Threshold { get; set; }

        /// <summary>
        /// Constructs default instance.
        /// </summary>
        protected NotifierSettings()
        {
            Threshold = LogLevel.Warn;
        }

        /// <summary>
        /// Converts these settings into an <see cref="INotifier"/>, effectively making this method
        /// a mini-composition-root.
        /// </summary>
        public abstract INotifier ToNotifier();
    }
}
