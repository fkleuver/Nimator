using System;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;

#pragma warning disable 4014

namespace Nimator.Tests
{
    public class HealthMonitorTests : IDisposable
    {
        public HealthMonitorTests()
        {
            HealthMonitor.Notifiers.Clear();
            HealthMonitor.Checks.Clear();
        }
        
        [NamedFact]
        public void StaticMethods_ShouldHaveCorrectGuardClauses()
        {
            typeof(HealthMonitor).VerifyStaticMethodGuards().Should().Be(2);
        }

        [NamedTheory, DefaultFixture]
        public void AddCheck_ShouldAddCheck(IHealthCheck check)
        {
            HealthMonitor.AddCheck(check).Should().BeTrue();

            HealthMonitor.Checks.Count.Should().Be(1);
        }

        [NamedTheory, DefaultFixture]
        public void AddCheck_ShouldAddCheckOnlyOnce_WhenSameCheckAddedMultipleTimes(IHealthCheck check)
        {
            HealthMonitor.AddCheck(check).Should().BeTrue();
            HealthMonitor.AddCheck(check).Should().BeFalse();
            HealthMonitor.AddCheck(check).Should().BeFalse();
            HealthMonitor.AddCheck(check).Should().BeFalse();

            HealthMonitor.Checks.Count.Should().Be(1);
        }

        [NamedTheory, DefaultFixture]
        public void AddNotifier_ShouldAddNotifier(INotifier notifier)
        {
            HealthMonitor.AddNotifier(notifier).Should().BeTrue();

            HealthMonitor.Notifiers.Count.Should().Be(1);
        }

        [NamedTheory, DefaultFixture]
        public void AddNotifier_ShouldAddNotifierOnlyOnce_WhenSameNotifierAddedMultipleTimes(INotifier notifier)
        {
            HealthMonitor.AddNotifier(notifier).Should().BeTrue();
            HealthMonitor.AddNotifier(notifier).Should().BeFalse();
            HealthMonitor.AddNotifier(notifier).Should().BeFalse();
            HealthMonitor.AddNotifier(notifier).Should().BeFalse();

            HealthMonitor.Notifiers.Count.Should().Be(1);
        }

        [NamedFact]
        public void Tick_ShouldNotThrow_WhenUnconfigured()
        {
            HealthMonitor.Tick();
        }

        [NamedTheory, DefaultFixture]
        public async void Tick_ShouldCallHealthCheck_WhenItNeedsToRun(IHealthCheck check)
        {
            check.NeedsToRun.Returns(true);
            HealthMonitor.AddCheck(check).Should().BeTrue();

            HealthMonitor.Tick();

            // wait for the queue to run
            await Task.Delay(5);

            check.Received(1).RunAsync();
        }

        [NamedTheory, DefaultFixture]
        public async void Tick_ShouldNotCallHealthCheck_WhenItDoesNotNeedToRun(IHealthCheck check)
        {
            check.NeedsToRun.Returns(false);
            HealthMonitor.AddCheck(check).Should().BeTrue();

            HealthMonitor.Tick();

            // wait for the queue to run
            await Task.Delay(5);

            check.DidNotReceive().RunAsync();
        }
        
        public void Dispose()
        {
            HealthMonitor.Notifiers.Clear();
            HealthMonitor.Checks.Clear();
        }
    }
}
