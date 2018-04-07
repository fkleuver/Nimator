using FluentAssertions;
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

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new JsonHealthCheckResultFormatter();
            typeof(JsonHealthCheckResultFormatter).VerifyInstanceMethodGuards(sut).Should().Be(1);
        }
    }
}
