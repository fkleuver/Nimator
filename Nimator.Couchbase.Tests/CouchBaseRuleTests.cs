using Couchbase.Core;
using FluentAssertions;
using Nimator.CouchBase;
using Nimator.Tests;
using Nimator.Util;

namespace Nimator.Couchbase.Tests
{
    public class CouchBaseRuleTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(CouchBaseRule<IClusterInfo>).VerifyConstructorGuards(CouchBaseFixture.CreateContext()).Should().Be(1);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new CouchBaseRule<IClusterInfo>(new Identity("foo"));
            typeof(CouchBaseRule<IClusterInfo>).VerifyInstanceMethodGuards(sut, CouchBaseFixture.CreateContext()).Should().Be(39);
        }
    }
}
