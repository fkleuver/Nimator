using FluentAssertions;
using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class OpsGenieNotifierTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(OpsGenieNotifier).VerifyConstructorGuards().Should().Be(1);
        }

        [NamedTheory, DefaultFixture]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses(OpsGenieNotifierSettings settings)
        {
            var sut = settings.ToNotifier();
            typeof(OpsGenieNotifier).VerifyInstanceMethodGuards(sut).Should().Be(1);
        }
    }
}
