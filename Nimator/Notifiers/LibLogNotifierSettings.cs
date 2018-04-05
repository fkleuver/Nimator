using Nimator.Logging;

namespace Nimator.Notifiers
{
    /// <inheritdoc />
    /// <summary>
    /// Settings for a <see cref="T:Nimator.Notifiers.ConsoleNotifier" />.
    /// </summary>
    public sealed class LibLogNotifierSettings : NotifierSettings
    {
        /// <summary>
        /// Constructs default settings
        /// </summary>
        public LibLogNotifierSettings()
        {
            Threshold = LogLevel.Info;
        }

        /// <inheritDoc />
        public override INotifier ToNotifier()
        {
            return new LibLogNotifier(this);
        }

        public static NotifierSettings Create()
        {
            return new LibLogNotifierSettings();
        }
    }
}
