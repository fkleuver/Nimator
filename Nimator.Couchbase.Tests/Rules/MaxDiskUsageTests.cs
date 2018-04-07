using FluentAssertions;
using Nimator.CouchBase.Rules;
using Nimator.Tests;

namespace Nimator.Couchbase.Tests.Rules
{
    public class MaxDiskUsageTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(MaxDiskUsage).VerifyConstructorGuards(CouchBaseFixture.CreateContext()).Should().Be(1);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = new MaxDiskUsage(0);
            typeof(MaxDiskUsage).VerifyInstanceMethodGuards(sut, CouchBaseFixture.CreateContext()).Should().Be(41);
        }
    }
}
