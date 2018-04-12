using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Nimator.Messaging;
using NSubstitute;

#pragma warning disable 4014

namespace Nimator.Tests
{
    public class HealthMonitorTests : IDisposable
    {
        public HealthMonitorTests()
        {
            EventAggregator.Instance.Dispose();
            HealthMonitor.Notifiers.Clear();
            HealthMonitor.Checks.Clear();
        }
        
        [NamedFact]
        public void StaticMethods_ShouldHaveCorrectGuardClauses()
        {
            typeof(HealthMonitor).VerifyStaticMethodGuards().Should().Be(5);
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
        public void RemoveCheck_ShouldRemoveCheck(IHealthCheck check)
        {
            HealthMonitor.AddCheck(check).Should().BeTrue();

            HealthMonitor.Checks.Count.Should().Be(1);

            HealthMonitor.RemoveCheck(check).Should().BeTrue();

            HealthMonitor.Checks.Count.Should().Be(0);
        }

        [NamedTheory, DefaultFixture]
        public void RemoveCheck_ShouldNotThrow_WhenCheckAlreadyRemoved(IHealthCheck check)
        {
            HealthMonitor.AddCheck(check).Should().BeTrue();

            HealthMonitor.Checks.Count.Should().Be(1);

            HealthMonitor.RemoveCheck(check).Should().BeTrue();

            HealthMonitor.Checks.Count.Should().Be(0);
            
            HealthMonitor.RemoveCheck(check).Should().BeFalse();
            HealthMonitor.RemoveCheck(check).Should().BeFalse();
            HealthMonitor.RemoveCheck(check).Should().BeFalse();
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

        [NamedTheory, DefaultFixture]
        public void RemoveNotifier_ShouldRemoveNotifier(INotifier notifier)
        {
            HealthMonitor.AddNotifier(notifier).Should().BeTrue();

            HealthMonitor.Notifiers.Count.Should().Be(1);

            HealthMonitor.RemoveNotifier(notifier).Should().BeTrue();

            HealthMonitor.Notifiers.Count.Should().Be(0);
        }

        [NamedTheory, DefaultFixture]
        public void RemoveNotifier_ShouldNotThrow_WhenNotifierAlreadyRemoved(INotifier notifier)
        {
            HealthMonitor.AddNotifier(notifier).Should().BeTrue();

            HealthMonitor.Notifiers.Count.Should().Be(1);

            HealthMonitor.RemoveNotifier(notifier).Should().BeTrue();

            HealthMonitor.Notifiers.Count.Should().Be(0);
            
            HealthMonitor.RemoveNotifier(notifier).Should().BeFalse();
            HealthMonitor.RemoveNotifier(notifier).Should().BeFalse();
            HealthMonitor.RemoveNotifier(notifier).Should().BeFalse();
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
            EventAggregator.Instance.Dispose();
            HealthMonitor.Notifiers.Clear();
            HealthMonitor.Checks.Clear();
        }
    }
}
