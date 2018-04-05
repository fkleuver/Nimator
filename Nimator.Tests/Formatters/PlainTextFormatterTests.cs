using Nimator.Formatters;

namespace Nimator.Tests.Formatters
{
    public class PlainTextFormatterTests
    {
        [NamedFact]
        public void Constructor_ShouldNotThrow()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new PlainTextFormatter();
        }
    }
}
