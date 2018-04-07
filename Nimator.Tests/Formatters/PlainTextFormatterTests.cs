using FluentAssertions;
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

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new PlainTextFormatter();
            typeof(PlainTextFormatter).VerifyInstanceMethodGuards(sut).Should().Be(1);
        }
    }
}
