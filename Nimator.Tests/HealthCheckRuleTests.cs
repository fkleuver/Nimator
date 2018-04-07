using System;
using FluentAssertions;
using Nimator.Logging;

namespace Nimator.Tests
{
    public class HealthCheckRuleTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(HealthCheckRule<string>).VerifyConstructorGuards().Should().Be(2);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new HealthCheckRule<string>("Foo");
            typeof(HealthCheckRule<string>).VerifyInstanceMethodGuards(sut).Should().Be(23);
        }

        [NamedFact]
        public void IsMatch_ShouldReturnFalse_WhenNotDataCollectionResult()
        {
            var sut = new HealthCheckRule<string>("Foo");

            sut.IsMatch("Bar").Should().BeFalse();
        }

        [NamedTheory, DefaultFixture]
        public void IsMatch_ShouldReturnFalse_WhenDataCollectionResult_AndUnAssignableType(DataCollectionResult<object> result)
        {
            var sut = new HealthCheckRule<string>("Foo");

            sut.IsMatch(result).Should().BeFalse();
        }

        [NamedTheory, DefaultFixture]
        public void IsMatch_ShouldReturnTrue_WhenDataCollectionResult_AndCorrectType(DataCollectionResult<string> result)
        {
            var sut = new HealthCheckRule<string>("Foo");

            sut.IsMatch(result).Should().BeTrue();
        }

        [NamedTheory, DefaultFixture]
        public void IsMatch_ShouldReturnTrue_WhenDataCollectionResult_AndAssignableType(DataCollectionResult<string> result)
        {
            var sut = new HealthCheckRule<object>("Foo");

            sut.IsMatch(result).Should().BeTrue();
        }

        [NamedTheory, DefaultFixture]
        public void GetResult_ShouldThrow_WhenGivenNonMatchingType(DataCollectionResult<object> result)
        {
            var sut = new HealthCheckRule<string>("Foo");

            Action act = () => sut.GetResult(result);

            act.Should().Throw<ArgumentException>();
        }

        [NamedTheory, DefaultFixture]
        public void GetResult_ShouldReturnResult_WhenCustomPredicateIsMatch(DataCollectionResult<string> result)
        {
            var sut = new HealthCheckRule<string>("Foo");
            sut.WhenResult(x => x.Success && x.Data == result.Data, sut.ApplyStandardOkayOperationalPolicy);

            var actual = sut.GetResult(result);
            actual.Level.Should().Be(LogLevel.Info);
            actual.Status.Should().Be(Status.Okay);
        }

        [NamedTheory, DefaultFixture]
        public void GetResult_ShouldReturnNoResult_WhenCustomPredicateIsNotMatch(DataCollectionResult<string> result)
        {
            var sut = new HealthCheckRule<string>("Foo");
            sut.WhenResult(x => x.Success && x.Data != result.Data, sut.ApplyStandardOkayOperationalPolicy);

            var actual = sut.GetResult(result);
            actual.Should().BeNull();
        }
    }
}
