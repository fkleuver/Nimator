using System;
using FluentAssertions;
using Nimator.Logging;
using Nimator.Rules;
using Nimator.Util;
using NSubstitute;

namespace Nimator.Tests.Rules
{
    public class DataCollectorErrorTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(DataCollectorError).VerifyConstructorGuards().Should().Be(2);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new DataCollectorError(new Identity("foo"));
            typeof(DataCollectorError).VerifyInstanceMethodGuards(sut).Should().Be(2);
        }

        [NamedTheory, DefaultFixture]
        public void IsMatch_ShouldReturnFalse_WhenDataResultHasNoError(IDataCollectionResult dataResult)
        {
            dataResult.Error.Returns((Exception)null);
            var sut = new DataCollectorError(new Identity("foo"));

            var actual = sut.IsMatch(dataResult);

            actual.Should().BeFalse();
        }

        [NamedTheory, DefaultFixture]
        public void IsMatch_ShouldReturnTrue_WhenDataResultHasError(IDataCollectionResult dataResult)
        {
            dataResult.Error.Returns(new Exception());
            var sut = new DataCollectorError(new Identity("foo"));

            var actual = sut.IsMatch(dataResult);

            actual.Should().BeTrue();
        }

        [NamedTheory, DefaultFixture]
        public void GetResult_ShouldThrow_WhenDataResultHasNoError(IDataCollectionResult dataResult)
        {
            dataResult.Error.Returns((Exception)null);
            var sut = new DataCollectorError(new Identity("foo"));

            Action act = () => sut.GetResult(dataResult);
            act.Should().Throw<ArgumentException>();
        }

        [NamedTheory, DefaultFixture]
        public void GetResult_ShouldCallStopProcessingOnDataResult_WhenDataResultHasError(MockDataCollectionResult dataResult)
        {
            dataResult.Mock.Error.Returns(new Exception());
            var sut = new DataCollectorError(new Identity("foo"));

            sut.GetResult(dataResult);

            dataResult.Mock.Received(1).StopProcessing();
        }

        [NamedTheory, DefaultFixture]
        public void GetResult_ShouldReturnFatalCritical_WhenDataCollectorTimedOut(IDataCollectionResult dataResult)
        {
            dataResult.Error.Returns(new TimeoutException());
            var sut = new DataCollectorError(new Identity("foo"));

            var actual = sut.GetResult(dataResult);
            
            actual.Status.Should().Be(Status.Critical);
            actual.Level.Should().Be(LogLevel.Fatal);
            actual.Exception.Should().BeOfType<TimeoutException>();
            actual.Reason.Should().Be($"The request to collect data from \"IDataCollector\" for \"foo\" timed out.");
        }

        [NamedTheory, DefaultFixture]
        public void GetResult_ShouldReturnFatalUnknown_WhenDataCollectorHasOtherError(IDataCollectionResult dataResult)
        {
            dataResult.Error.Returns(new Exception());
            var sut = new DataCollectorError(new Identity("foo"));

            var actual = sut.GetResult(dataResult);
            
            actual.Status.Should().Be(Status.Unknown);
            actual.Level.Should().Be(LogLevel.Fatal);
            actual.Exception.Should().BeOfType<Exception>();
            actual.Reason.Should().Be($"Nimator failed while trying to collect data from \"IDataCollector\" for \"foo\".");
        }
    }
}
