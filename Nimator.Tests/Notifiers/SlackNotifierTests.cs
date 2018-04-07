using FluentAssertions;
using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class SlackNotifierTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(SlackNotifier).VerifyConstructorGuards().Should().Be(1);
        }

        [NamedTheory, DefaultFixture]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses(SlackNotifierSettings settings)
        {
            var sut = settings.ToNotifier();
            typeof(SlackNotifier).VerifyInstanceMethodGuards(sut).Should().Be(1);
        }
    }
}
