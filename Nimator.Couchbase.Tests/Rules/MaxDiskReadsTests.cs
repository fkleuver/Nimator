using FluentAssertions;
using Nimator.CouchBase.Rules;
using Nimator.Tests;

namespace Nimator.Couchbase.Tests.Rules
{
    public class MaxDiskReadsTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(MaxDiskReads).VerifyConstructorGuards(CouchBaseFixture.CreateContext()).Should().Be(1);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new MaxDiskReads(0);
            typeof(MaxDiskReads).VerifyInstanceMethodGuards(sut, CouchBaseFixture.CreateContext()).Should().Be(41);
        }
    }
}
