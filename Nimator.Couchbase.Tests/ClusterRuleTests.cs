using FluentAssertions;
using Nimator.CouchBase;
using Nimator.Tests;
using Nimator.Util;

namespace Nimator.Couchbase.Tests
{
    public class ClusterRuleTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(ClusterRule).VerifyConstructorGuards(CouchBaseFixture.CreateContext()).Should().Be(1);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new ClusterRule(new Identity("foo"));
            typeof(ClusterRule).VerifyInstanceMethodGuards(sut, CouchBaseFixture.CreateContext()).Should().Be(39);
        }
    }
}
