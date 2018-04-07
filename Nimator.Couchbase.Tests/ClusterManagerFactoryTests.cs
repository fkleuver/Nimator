using FluentAssertions;
using Nimator.CouchBase;
using Nimator.Tests;

namespace Nimator.Couchbase.Tests
{
    public class ClusterManagerFactoryTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(ClusterManagerFactory).VerifyConstructorGuards(CouchBaseFixture.CreateContext()).Should().Be(13);
        }
    }
}
