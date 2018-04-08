using System;
using Couchbase;
using FluentAssertions;
using Nimator.CouchBase.Rules;
using Nimator.Logging;
using Nimator.Rules;
using Nimator.Tests;
using Nimator.Util;
using NSubstitute;

namespace Nimator.Couchbase.Tests.Rules
{
    public class ServiceUnresponsiveTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(ServiceUnresponsive).VerifyConstructorGuards(CouchBaseFixture.CreateContext()).Should().Be(2);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new ServiceUnresponsive("foo");
            typeof(ServiceUnresponsive).VerifyInstanceMethodGuards(sut, CouchBaseFixture.CreateContext()).Should().Be(2);
        }

        [NamedTheory, CouchBaseFixture]
        public void IsMatch_ShouldReturnFalse_WhenDataIsNotOfTypeIResult(Identity checkId, DataCollectionResult<object> dataResult)
        {
            var sut = new ServiceUnresponsive(checkId);
            const bool expected = false;

            var actual = sut.IsMatch(dataResult);

            actual.Should().Be(expected);
        }

        [NamedTheory, CouchBaseFixture]
        public void IsMatch_ShouldReturnTrue_WhenDataIsOfTypeIResult(Identity checkId, DataCollectionResult<IResult<object>> dataResult)
        {
            var sut = new ServiceUnresponsive(checkId);
            const bool expected = true;

            var actual = sut.IsMatch(dataResult);

            actual.Should().Be(expected);
        }

        [NamedTheory, CouchBaseFixture]
        public void GetResult_ShouldCallStopProcessingOnDataResult_WhenDataResultHasError(Identity checkId, MockDataCollectionResult dataResult, MockResult<object> result)
        {
            var sut = new ServiceUnresponsive(checkId);

            dataResult.Mock.Data.Returns(result);
            result.Mock.Success.Returns(false);
            
            sut.GetResult(dataResult);

            dataResult.Mock.Received(1).StopProcessing();
        }

        [NamedTheory, CouchBaseFixture]
        public void GetResult_ShouldReturnCriticalError_WhenIResultSuccessIsFalse(Identity checkId, MockDataCollectionResult dataResult, MockResult<object> result)
        {
            var sut = new ServiceUnresponsive(checkId);

            dataResult.Mock.Data.Returns(result);
            result.Mock.Success.Returns(false);

            var actual = sut.GetResult(dataResult);
            
            actual.Status.Should().Be(Status.Critical);
            actual.Level.Should().Be(LogLevel.Error);
            actual.Reason.Should().Be($"Service did not respond to request from \"{dataResult.Origin.Id.Name}\".");
        }

        [NamedTheory, CouchBaseFixture]
        public void GetResult_ShouldReturnNull_WhenIResultSuccessIsTrue(Identity checkId, MockDataCollectionResult dataResult, MockResult<object> result)
        {
            var sut = new ServiceUnresponsive(checkId);

            dataResult.Mock.Data.Returns(result);
            result.Mock.Success.Returns(true);

            var actual = sut.GetResult(dataResult);

            actual.Should().BeNull();
        }
    }
}
