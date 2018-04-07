using FluentAssertions;
using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class LibLogNotifierTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(LibLogNotifier).VerifyConstructorGuards().Should().Be(1);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = LibLogNotifierSettings.Create().ToNotifier();
            typeof(LibLogNotifier).VerifyInstanceMethodGuards(sut).Should().Be(1);
        }
    }
}
