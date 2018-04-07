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
        public async void Tick_ShouldCallHealthCheck_WhenItNeedsToRun_AndIsNotRunning(IHealthCheck check)
        {
            check.IsRunning.Returns(false);
            check.NeedsToRun.Returns(true);
            HealthMonitor.AddCheck(check).Should().BeTrue();

            HealthMonitor.Tick();

            // wait for the queue to run
            await Task.Delay(5);

            check.Received(1).RunAsync();
        }

        [NamedTheory, DefaultFixture]
        public async void Tick_ShouldNotCallHealthCheck_WhenItDoesNotNeedToRun_AndIsNotRunning(IHealthCheck check)
        {
            check.IsRunning.Returns(false);
            check.NeedsToRun.Returns(false);
            HealthMonitor.AddCheck(check).Should().BeTrue();

            HealthMonitor.Tick();

            // wait for the queue to run
            await Task.Delay(5);

            check.DidNotReceive().RunAsync();
        }

        [NamedTheory, DefaultFixture]
        public async void Tick_ShouldNotCallHealthCheck_WhenItDoesNotNeedToRun_AndIsRunning(IHealthCheck check)
        {
            check.IsRunning.Returns(true);
            check.NeedsToRun.Returns(false);
            HealthMonitor.AddCheck(check).Should().BeTrue();

            HealthMonitor.Tick();

            // wait for the queue to run
            await Task.Delay(5);

            check.DidNotReceive().RunAsync();
        }

        [NamedTheory, DefaultFixture]
        public async void Tick_ShouldNotCallHealthCheck_WhenItNeedsToRun_AndIsRunning(IHealthCheck check)
        {
            check.IsRunning.Returns(true);
            check.NeedsToRun.Returns(true);
            HealthMonitor.AddCheck(check).Should().BeTrue();

            HealthMonitor.Tick();

            // wait for the queue to run
            await Task.Delay(5);

            check.DidNotReceive().RunAsync();
        }

        
        #region Threading related tests
        
        /// <summary>
        /// This is more of an integration test rather than a unit test, but that's kind of the point since the thread-safety
        /// of the system is dependant on different components doing their part of the checking.
        /// </summary>
        /// <param name="check"></param>
        [NamedTheory, DefaultFixture]
        public async void Tick_ShouldOnlyCallHealthCheckOnce_WhenCalledFrom20ParallelThreads(HealthCheck check)
        {
            var collector = Substitute.For<IDataCollector>();
            collector.IsRunning.Returns(false);
            collector.NeedsToRun.Returns(true);
            collector.When(x => x.GetAsync()).Do(x => Task.FromResult((object)null));
            check.AddDataCollector(collector);
            HealthMonitor.AddCheck(check).Should().BeTrue();
            
            const int count = 20;
            var threads = new Thread[count];
            for (var i = 0; i < count; i++)
            {
                threads[i] = new Thread(HealthMonitor.Tick);
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Start();
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Join();
            }

            // wait for the queue to run
            await Task.Delay(10);

            collector.Received(1).GetAsync();
        }
        
        /// <summary>
        /// This is not really a test in itself, but rather meant to rule out false positives for the above test
        /// </summary>
        /// <param name="check"></param>
        [NamedTheory, DefaultFixture]
        public async void Tick_ShouldCallHealthCheckEachTime_WhenCalledFrom20ParallelThreads_AndHasNoDataCollector(IHealthCheck check)
        {
            check.NeedsToRun.Returns(true);
            check.IsRunning.Returns(false);
            HealthMonitor.AddCheck(check).Should().BeTrue();
            
            const int count = 20;
            var threads = new Thread[count];
            for (var i = 0; i < count; i++)
            {
                threads[i] = new Thread(HealthMonitor.Tick);
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Start();
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Join();
            }

            // wait for the queue to run
            await Task.Delay(10);

            check.Received(20).RunAsync();
        }
        
          #endregion

        public void Dispose()
        {
            EventAggregator.Instance.Dispose();
            HealthMonitor.Notifiers.Clear();
            HealthMonitor.Checks.Clear();
        }
    }
}
