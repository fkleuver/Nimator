using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class ConsoleNotifierSettingsTests
    {
        [NamedFact]
        public void Constructor_ShouldNotThrow()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new ConsoleNotifierSettings();
        }
    }
}
