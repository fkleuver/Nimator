using Couchbase;
using Couchbase.Core;
using FluentAssertions;
using Nimator.CouchBase.Rules;
using Nimator.Logging;
using Nimator.Tests;
using Nimator.Util;

namespace Nimator.Couchbase.Tests.Rules
{
    public class MinAvailablePoolMemoryQuotaPercentageTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(MinAvailablePoolMemoryQuotaPercentage).VerifyConstructorGuards(CouchBaseFixture.CreateContext()).Should().Be(1);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new MinAvailablePoolMemoryQuotaPercentage(0);
            typeof(MinAvailablePoolMemoryQuotaPercentage).VerifyInstanceMethodGuards(sut, CouchBaseFixture.CreateContext()).Should().Be(39);
        }

        [NamedTheory, CouchBaseFixture]
        public void IsMatch_ShouldReturnFalse_WhenDataIsNotOfTypePool(Identity checkId, int percentage, object data)
        {
            var sut = new MinAvailablePoolMemoryQuotaPercentage(checkId, percentage);
            const bool expected = false;

            var actual = sut.IsMatch(data);

            actual.Should().Be(expected);
        }

        [NamedTheory, CouchBaseFixture]
        public void IsMatch_ShouldReturnTrue_WhenDataIsOfTypePool(Identity checkId, int percentage, DataCollectionResult<IResult<IClusterInfo>> cluster)
        {
            var sut = new MinAvailablePoolMemoryQuotaPercentage(checkId, percentage);
            const bool expected = true;

            var actual = sut.IsMatch(cluster);

            actual.Should().Be(expected);
        }

        [NamedTheory, CouchBaseFixture]
        public void GetResult_PoolHasMoreThanSpecifiedPercentage_ReturnsOkayStatus(Identity checkId,  DataCollectionResult<IResult<IClusterInfo>> cluster)
        {
            const int minPercentage = 15;
            cluster.Data.Value.Pools().StorageTotals.Ram.QuotaTotal = 1000000;
            cluster.Data.Value.Pools().StorageTotals.Ram.QuotaUsed = 840000;

            var sut = new MinAvailablePoolMemoryQuotaPercentage(checkId, minPercentage);

            var actual = sut.GetResult(cluster);
            actual.InnerResults.Count.Should().Be(0);
            actual.Status.Should().Be(Status.Okay);
            actual.Level.Should().Be(LogLevel.Info);
        }

        [NamedTheory, CouchBaseFixture]
        public void GetResult_PoolHasLessThanSpecifiedPercentage_ReturnsWarnStatus(Identity checkId, DataCollectionResult<IResult<IClusterInfo>> cluster)
        {
            const int minPercentage = 15;
            cluster.Data.Value.Pools().StorageTotals.Ram.QuotaTotal = 1000000;
            cluster.Data.Value.Pools().StorageTotals.Ram.QuotaUsed = 860000;

            var sut = new MinAvailablePoolMemoryQuotaPercentage(checkId, minPercentage);

            var actual = sut.GetResult(cluster);
            actual.InnerResults.Count.Should().Be(0);
            actual.Status.Should().Be(Status.Warning);
            actual.Level.Should().Be(LogLevel.Warn);
        }
    }
}
