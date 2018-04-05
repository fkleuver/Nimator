using Nimator.Formatters;

namespace Nimator.Tests.Formatters
{
    public class JsonHealthCheckResultFormatterTests
    {
        [NamedFact]
        public void Constructor_ShouldNotThrow()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new JsonHealthCheckResultFormatter();
        }
    }
}
