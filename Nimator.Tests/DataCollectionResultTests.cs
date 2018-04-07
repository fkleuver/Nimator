using FluentAssertions;

namespace Nimator.Tests
{
    public class DataCollectionResultTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(DataCollectionResult<DummyData>).VerifyConstructorGuards().Should().Be(4);
        }
    }
}
