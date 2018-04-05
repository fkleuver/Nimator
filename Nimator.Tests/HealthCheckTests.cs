using System;
using FluentAssertions;
using NSubstitute;

#pragma warning disable 4014

namespace Nimator.Tests
{
    public class HealthCheckTests
    {
        private IDataCollector GetDataCollectorSubstitute()
        {
            var collector = Substitute.For<IDataCollector>();
            collector.IsRunning.Returns(false);
            collector.NeedsToRun.Returns(true);
            return collector;
        }

        [NamedFact]
        public void Constructor_ShouldThrow_WhenNameIsNull()
        {
            Action act = () => new HealthCheck((string)null);

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedFact]
        public async void RunAsync_ShouldCallDataCollector()
        {
            var sut = new HealthCheck();
            for (var i = 0; i < 10; i++)
            {
                sut.AddDataCollector(GetDataCollectorSubstitute());
            }

            await sut.RunAsync();
            
            foreach (var collector in sut.DataCollectors)
            {
                collector.Received(1).GetAsync();
            }
        }
        
        [NamedFact]
        public async void RunAsync_ShouldCallDataCollector_WhenItNeedsToRun_AndIsNotRunning()
        {
            var sut = new HealthCheck();
            var collector = Substitute.For<IDataCollector>();
            collector.IsRunning.Returns(false);
            collector.NeedsToRun.Returns(true);
            sut.AddDataCollector(collector);

            await sut.RunAsync().ConfigureAwait(false);

            collector.Received(1).GetAsync();
        }

        [NamedFact]
        public async void RunAsync_ShouldNotCallDataCollector_WhenItDoesNotNeedToRun_AndIsNotRunning()
        {
            var sut = new HealthCheck();
            var collector = Substitute.For<IDataCollector>();
            collector.IsRunning.Returns(false);
            collector.NeedsToRun.Returns(false);
            sut.AddDataCollector(collector);
            
            await sut.RunAsync().ConfigureAwait(false);

            collector.DidNotReceive().GetAsync();
        }

        [NamedFact]
        public async void RunAsync_ShouldNotCallDataCollector_WhenItDoesNotNeedToRun_AndIsRunning()
        {
            var sut = new HealthCheck();
            var collector = Substitute.For<IDataCollector>();
            collector.IsRunning.Returns(true);
            collector.NeedsToRun.Returns(false);
            sut.AddDataCollector(collector);
            
            await sut.RunAsync().ConfigureAwait(false);

            collector.DidNotReceive().GetAsync();
        }

        [NamedFact]
        public async void RunAsync_ShouldNotCallDataCollector_WhenItNeedsToRun_AndIsRunning()
        {
            var sut = new HealthCheck();
            var collector = Substitute.For<IDataCollector>();
            collector.IsRunning.Returns(true);
            collector.NeedsToRun.Returns(true);
            sut.AddDataCollector(collector);
            
            await sut.RunAsync().ConfigureAwait(false);

            collector.DidNotReceive().GetAsync();
        }

        [NamedFact]
        public async void RunAsync_ShouldNotThrow_WhenDataCollectorThrows()
        {
            var sut = new HealthCheck();
            var collector = Substitute.For<IDataCollector>();
            collector.IsRunning.Returns(false);
            collector.NeedsToRun.Returns(true);
            sut.AddDataCollector(collector);
            collector.When(x => x.GetAsync()).Throw<Exception>();
            
            await sut.RunAsync().ConfigureAwait(false);
        }
    }
}
