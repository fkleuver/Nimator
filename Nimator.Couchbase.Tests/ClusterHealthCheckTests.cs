using System.Collections.Generic;
using Couchbase;
using Couchbase.Configuration.Server.Serialization;
using Couchbase.Core;
using FluentAssertions;
using Nimator.CouchBase;
using Nimator.Tests;

namespace Nimator.Couchbase.Tests
{
    public class ClusterHealthCheckTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(ClusterHealthCheck).VerifyConstructorGuards(CouchBaseFixture.CreateContext()).Should().Be(4);
        }
        
        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldAddTwoCorrectDataCollectorsForSingleFactory(ClusterManagerFactory factory)
        {
            var sut = new ClusterHealthCheck(factory);

            sut.DataCollectors.Count.Should().Be(2);
            sut.DataCollectors.Should().Contain(d => d is DataCollector<IResult<IList<BucketConfig>>>);
            sut.DataCollectors.Should().Contain(d => d is DataCollector<IResult<IClusterInfo>>);
        }
        
        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldAddSixCorrectDataCollectorsForThreeFactories(IEnumerable<ClusterManagerFactory> factories)
        {
            var sut = new ClusterHealthCheck(factories);

            sut.DataCollectors.Count.Should().Be(6);
            sut.DataCollectors.Should().Contain(d => d is DataCollector<IResult<IList<BucketConfig>>>);
            sut.DataCollectors.Should().Contain(d => d is DataCollector<IResult<IClusterInfo>>);
        }
    }
}
