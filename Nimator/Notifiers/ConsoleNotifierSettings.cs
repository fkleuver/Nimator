using System;
using Nimator.Formatters;
using Nimator.Logging;

namespace Nimator.Notifiers
{
    /// <summary>
    /// Settings for a <see cref="T:Nimator.Notifiers.ConsoleNotifier" />.
    /// </summary>
    public sealed class ConsoleNotifierSettings : NotifierSettings
    {
        /// <summary>
        /// Constructs default settings
        /// </summary>
        public ConsoleNotifierSettings()
        {
            Threshold = LogLevel.Info;
        }

        /// <inheritDoc/>
        public override INotifier ToNotifier()
        {
            return new ConsoleNotifier(this, new JsonHealthCheckResultFormatter(), Console.WriteLine);
        }

        public static NotifierSettings Create()
        {
            return new ConsoleNotifierSettings();
        }
    }
}
