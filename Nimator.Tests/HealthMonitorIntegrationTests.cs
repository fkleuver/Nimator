using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Nimator.Logging;
using Nimator.Messaging;
using Nimator.Util;

namespace Nimator.Tests
{
    public class HealthMonitorIntegrationTests : IDisposable
    {
        public HealthMonitorIntegrationTests()
        {
            HealthMonitor.Checks.Clear();
            HealthMonitor.Notifiers.Clear();
            EventAggregator.Instance.Dispose();
        }

        [NamedFact]
        public async void Tick_ShouldPublishCorrectResults_WhenGivenSeveralRulesAndDataCollectors()
        {
            var check1 = new HealthCheck("Check1");
            check1.AddDataCollector(() => Task.FromResult("Foo1"));
            check1.AddDataCollector(() => Task.FromResult("Bar1"));
            check1.AddDataCollector(() => Task.FromResult("Baz1"));
            check1.AddDataCollector(() => Task.FromResult("Qux1"));
            check1.AddRule<string>(rule => rule.WhenResult(r => r.Success && r.Data == "Foo1", (health, result) => health.SetLevel(LogLevel.Info).SetStatus(Status.Okay).SetReason("Foo1")));
            check1.AddRule<string>(rule => rule.WhenResult(r => r.Success && r.Data == "Bar1", (health, result) => health.SetLevel(LogLevel.Info).SetStatus(Status.Okay).SetReason("Bar1")));
            check1.AddRule<string>(rule => rule.WhenResult(r => r.Success && r.Data == "Baz1", (health, result) => health.SetLevel(LogLevel.Info).SetStatus(Status.Okay).SetReason("Baz1")));
            check1.AddRule<string>(rule => rule.WhenResult(r => r.Success && r.Data == "Qux1", (health, result) => health.SetLevel(LogLevel.Warn).SetStatus(Status.Maintenance).SetReason("Qux1")));
            
            HealthMonitor.AddCheck(check1);

            var bag = new ConcurrentBag<HealthCheckResult>();
            EventAggregator.Instance.Subscribe<HealthCheckResult>(result =>
            {
                bag.Add(result);
            });

            HealthMonitor.Tick();

            // ensure everything ran
            await Task.Delay(200);
            
            bag.Count.Should().Be(1);
            var result1 = bag.Single(r => r.CheckId.Name == "Check1");

            result1.InnerResults.Count.Should().Be(5);
            
            var result1Foo1 = result1.InnerResults.Single(r => r.Reason == "Foo1");
            var result1Bar1 = result1.InnerResults.Single(r => r.Reason == "Bar1");
            var result1Baz1 = result1.InnerResults.Single(r => r.Reason == "Baz1");
            var result1Qux1 = result1.InnerResults.Single(r => r.Reason == "Qux1");
            
            result1Foo1.Status.Should().Be(Status.Okay);
            result1Bar1.Status.Should().Be(Status.Okay);
            result1Baz1.Status.Should().Be(Status.Okay);
            result1Qux1.Status.Should().Be(Status.Maintenance);
        }
        [NamedFact]
        public async void Tick_ShouldPublishCorrectResults_WhenGivenSeveralRulesAndDataCollectors_AndSomeOfThemTimeOut()
        {
            var check2 = new HealthCheck("Check2");
            check2.AddDataCollector(async () =>
            {
                await Task.Delay(10);
                return "Foo2";
            }, null, TimeSpan.FromMilliseconds(50));
            check2.AddDataCollector(async () =>
            {
                await Task.Delay(10);
                return "Bar2";
            }, null, TimeSpan.FromMilliseconds(50));
            check2.AddDataCollector(async () =>
            {
                await Task.Delay(100);
                return "Baz2";
            }, null, TimeSpan.FromMilliseconds(50));
            check2.AddDataCollector(async () =>
            {
                await Task.Delay(100);
                return "Qux2";
            }, null, TimeSpan.FromMilliseconds(50));
            check2.AddRule(HealthCheckRule<string>.Create(new Identity("Foo2")).WhenResult(r => r.Success && r.Data == "Foo2", (health, result) => health.SetLevel(LogLevel.Info).SetStatus(Status.Okay).SetReason("Foo2")));
            check2.AddRule(HealthCheckRule<string>.Create(new Identity("Bar2")).WhenResult(r => r.Success && r.Data == "Bar2", (health, result) => health.SetLevel(LogLevel.Info).SetStatus(Status.Okay).SetReason("Bar2")));
            check2.AddRule(HealthCheckRule<string>.Create(new Identity("Baz2")).WhenResult(r => r.Success && r.Data == "Baz2", (health, result) => health.SetLevel(LogLevel.Info).SetStatus(Status.Okay).SetReason("Baz2")));
            check2.AddRule(HealthCheckRule<string>.Create(new Identity("Qux2")).WhenResult(r => r.Success && r.Data == "Qux2", (health, result) => health.SetLevel(LogLevel.Warn).SetStatus(Status.Maintenance).SetReason("Qux2")));
            
            HealthMonitor.AddCheck(check2);

            var bag = new ConcurrentBag<HealthCheckResult>();
            EventAggregator.Instance.Subscribe<HealthCheckResult>(result =>
            {
                bag.Add(result);
            });
            
            HealthMonitor.Tick();

            // ensure everything ran
            await Task.Delay(200);
            
            bag.Count.Should().Be(1);

            var result2 = bag.Single(r => r.CheckId.Name == "Check2");

            result2.InnerResults.Count.Should().Be(6);
            
            var result2Foo2 = result2.InnerResults.Single(r => r.Reason == "Foo2");
            var result2Bar2 = result2.InnerResults.Single(r => r.Reason == "Bar2");
            result2Foo2.Status.Should().Be(Status.Okay);
            result2Bar2.Status.Should().Be(Status.Okay);

            // baz2 & qux2 should not be matched since their collectors timed out
            result2.InnerResults.Any(r => r.Reason == "Baz2").Should().BeFalse();
            result2.InnerResults.Any(r => r.Reason == "Qux2").Should().BeFalse();

            // 2x timeout error
            result2.InnerResults.Count(r =>
                    r.Status == Status.Critical &&
                    r.Details[Constants.Exception].GetType() == typeof(TimeoutException))
                .Should().Be(2);

            // 2x warning for unmatched rule (baz2 & qux2)
            result2.InnerResults.Count(r =>
                    r.Status == Status.Unknown &&
                    r.Level == LogLevel.Warn)
                .Should().Be(2);
        }

        public void Dispose()
        {
            HealthMonitor.Checks.Clear();
            HealthMonitor.Notifiers.Clear();
            EventAggregator.Instance.Dispose();
        }
    }
}
