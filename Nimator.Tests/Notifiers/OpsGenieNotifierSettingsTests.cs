using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class OpsGenieNotifierSettingsTests
    {
        [NamedFact]
        public void Constructor_ShouldNotThrow()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new OpsGenieNotifierSettings();
        }
    }
}
