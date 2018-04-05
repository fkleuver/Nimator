using System.Collections.Generic;
using System.Linq;
using Couchbase;
using Couchbase.Configuration.Server.Serialization;
using FluentAssertions;
using Nimator.CouchBase.Rules;
using Nimator.Logging;
using Nimator.Tests;
using Nimator.Util;

namespace Nimator.Couchbase.Tests
{
    public class MaxTotalDocumentsInBucketTests
    {
        [NamedTheory, CouchBaseFixture]
        public void IsMatch_ShouldReturnFalse_WhenDataIsNotOfTypeBuckets(Identity checkId, long maxDocuments, object data)
        {
            var sut = new MaxTotalDocumentsInBucket(checkId, maxDocuments);
            const bool expected = false;

            var actual = sut.IsMatch(data);

            actual.Should().Be(expected);
        }

        [NamedTheory, CouchBaseFixture]
        public void IsMatch_ShouldReturnTrue_WhenDataIsOfTypeIListBuckets(Identity checkId, long maxDocuments, DataCollectionResult<IResult<IList<BucketConfig>>> buckets)
        {
            var sut = new MaxTotalDocumentsInBucket(checkId, maxDocuments);
            const bool expected = true;

            var actual = sut.IsMatch(buckets);

            actual.Should().Be(expected);
        }

        [NamedTheory, CouchBaseFixture]
        public void GetResult_WhenBucketHasLessThanSpecifiedDocuments_ReturnsOkayStatus(Identity checkId, DataCollectionResult<IResult<IList<BucketConfig>>> result)
        {
            result.Data.Value.Count.Should().Be(3);
            var maxDocuments = result.Data.Value.Max(b => b.BasicStats.ItemCount) + 1;
            var sut = new MaxTotalDocumentsInBucket(checkId, maxDocuments);

            var actual = sut.GetResult(result);
            actual.InnerResults.Count.Should().Be(3);
            foreach (var inner in actual.InnerResults)
            {
                inner.InnerResults.Count.Should().Be(0);
                inner.Status.Should().Be(Status.Okay);
                inner.Level.Should().Be(LogLevel.Info);
            }
        }

        [NamedTheory, CouchBaseFixture]
        public void GetResult_WhenBucketHasMoreThanSpecifiedDocuments_ReturnsWarnStatus(Identity checkId, DataCollectionResult<IResult<IList<BucketConfig>>> result)
        {
            result.Data.Value.Count.Should().Be(3);
            var maxDocuments = result.Data.Value.Min(b => b.BasicStats.ItemCount) - 1;
            var sut = new MaxTotalDocumentsInBucket(checkId, maxDocuments);

            var actual = sut.GetResult(result);
            actual.InnerResults.Count.Should().Be(3);
            foreach (var inner in actual.InnerResults)
            {
                inner.InnerResults.Count.Should().Be(0);
                inner.Status.Should().Be(Status.Warning);
                inner.Level.Should().Be(LogLevel.Warn);
            }
        }
    }
}
