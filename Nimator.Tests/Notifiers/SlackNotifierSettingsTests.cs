using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class SlackNotifierSettingsTests
    {
        [NamedFact]
        public void Constructor_ShouldNotThrow()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new SlackNotifierSettings();
        }
    }
}
