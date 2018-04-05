using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Nimator.Util;

namespace Nimator.Tests
{
    public class DataCollectorTests
    {
        [NamedFact]
        public void Constructor_ShouldThrow_WhenTaskFactoryIsNull()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Action act = () => new DataCollector<DummyData>(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldAssignCorrectPropertyValues(TimeSpan cacheDuration, Func<Task<DummyData>> taskFactory)
        {
            var sut = new DataCollector<DummyData>(taskFactory, cacheDuration);

            sut.Id.Name.Should().BeEquivalentTo(@"DataCollector<DummyData>");
            sut.CacheDuration.Should().Be(cacheDuration);
            sut.IsRunning.Should().BeFalse();
            sut.IsStale.Should().BeTrue();
            sut.LastRun.HasValue.Should().BeFalse();
            sut.NeedsToRun.Should().BeTrue();
            sut.NextRun.HasValue.Should().BeFalse();
            sut.Result.Should().BeNull();
        }

        [NamedFact]
        public void IsRunning_ShouldBeTrue_WhenIsRunning()
        {
            async Task<DummyData> TaskFactory()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                return new DummyData();
            }

            var sut = new DataCollector<DummyData>(TaskFactory);

#pragma warning disable 4014
            sut.GetAsync(); // Deliberately not calling await here, so that we capture the state as if it were still doing work
#pragma warning restore 4014

            sut.IsRunning.Should().BeTrue();
        }

        [NamedFact]
        public async void IsRunning_ShouldBeFalse_WhenIsNotRunning()
        {
            async Task<DummyData> TaskFactory()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                return new DummyData();
            }

            var sut = new DataCollector<DummyData>(TaskFactory);

            await sut.GetAsync();

            sut.IsRunning.Should().BeFalse();
        }

        [NamedFact]
        public void NeedsToRun_ShouldBeFalse_WhenIsRunning_AndStale()
        {
            async Task<DummyData> TaskFactory()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                return new DummyData();
            }

            var sut = new DataCollector<DummyData>(TaskFactory);

#pragma warning disable 4014
            sut.GetAsync(); // Deliberately not calling await here, so that we capture the state as if it were still doing work
#pragma warning restore 4014

            sut.IsStale.Should().BeTrue();
            sut.IsRunning.Should().BeTrue();
            sut.NeedsToRun.Should().BeFalse();
        }

        [NamedTheory, DefaultFixture]
        public async void NeedsToRun_ShouldBeFalse_WhenNotRunning_AndNotStale(Func<Task<DummyData>> taskFactory)
        {
            var cacheDuration = TimeSpan.FromSeconds(60);
            var sut = new DataCollector<DummyData>(taskFactory, cacheDuration);

            await sut.GetAsync();

            sut.IsStale.Should().BeFalse();
            sut.IsRunning.Should().BeFalse();
            sut.NeedsToRun.Should().BeFalse();
        }

        [NamedTheory, DefaultFixture]
        public async void NeedsToRun_ShouldBeTrue_WhenNotRunning_AndStale(Func<Task<DummyData>> taskFactory)
        {
            var sut = new DataCollector<DummyData>(taskFactory);

            await sut.GetAsync();

            sut.IsStale.Should().BeTrue();
            sut.IsRunning.Should().BeFalse();
            sut.NeedsToRun.Should().BeTrue();
        }

        [NamedTheory, DefaultFixture]
        public async void IsStale_ShouldBeFalse_WhenWithinCacheDuration(Func<Task<DummyData>> taskFactory)
        {
            var cacheDuration = TimeSpan.FromSeconds(60);
            var sut = new DataCollector<DummyData>(taskFactory, cacheDuration);

            await sut.GetAsync();

            sut.IsStale.Should().BeFalse();
        }

        [NamedTheory, DefaultFixture]
        public async void IsStale_ShouldBeTrue_WhenCacheExpiredd(Func<Task<DummyData>> taskFactory)
        {
            var sut = new DataCollector<DummyData>(taskFactory);

            await sut.GetAsync();

            sut.IsStale.Should().BeTrue();
        }

        [NamedTheory, DefaultFixture]
        public async void NextRun_ShouldBeNowPlusCacheDuration_WhenJustRan(Func<Task<DummyData>> taskFactory)
        {
            var cacheDuration = TimeSpan.FromSeconds(60);
            var acceptableDeviation = TimeSpan.FromSeconds(1).Ticks;
            var sut = new DataCollector<DummyData>(taskFactory, cacheDuration);
            var now = DateTimeProvider.GetSystemTimePrecise().Ticks;
            var expected = now + cacheDuration.Ticks;

            await sut.GetAsync();

            // ReSharper disable once PossibleInvalidOperationException
            sut.NextRun.Value.Should().BeInRange(expected - acceptableDeviation, expected + acceptableDeviation);
        }

        [NamedTheory, DefaultFixture]
        public async void LastRun_ShouldBeNow_WhenJustRan(Func<Task<DummyData>> taskFactory)
        {
            var acceptableDeviation = TimeSpan.FromSeconds(1).Ticks;
            var sut = new DataCollector<DummyData>(taskFactory);
            var expected = DateTimeProvider.GetSystemTimePrecise().Ticks;

            await sut.GetAsync();

            // ReSharper disable once PossibleInvalidOperationException
            sut.LastRun.Value.Should().BeInRange(expected - acceptableDeviation, expected + acceptableDeviation);
        }

        [NamedTheory, DefaultFixture]
        public async void GetAsync_ShouldReturnResultWithCorrectProperties(TimeSpan cacheDuration, DummyData expectedData)
        {
            async Task<DummyData> TaskFactory() => await Task.FromResult(expectedData);
            var sut = new DataCollector<DummyData>(TaskFactory, cacheDuration);
            var now = DateTimeProvider.GetSystemTimePrecise().Ticks;

            var actual = await sut.GetAsync();
            actual.Data.Should().Be(expectedData);
            actual.Origin.Should().Be(sut);
            actual.End.Should().Be(sut.LastRun);
            actual.Start.Should().BeGreaterThan(now);
            actual.Error.Should().BeNull();
        }

        [NamedTheory, DefaultFixture]
        public async void GetAsync_ShouldReturnDifferentResultsForConsecutiveCalls_WhenCacheDurationIsZero(DummyData expectedData)
        {
            async Task<DummyData> TaskFactory() => await Task.FromResult(expectedData);
            var sut = new DataCollector<DummyData>(TaskFactory);

            var actual1 = await sut.GetAsync();
            var actual2 = await sut.GetAsync();

            actual1.Should().NotBe(actual2);
            actual1.Data.Should().Be(actual2.Data); // should still be the same data though
        }
        
        [NamedTheory, DefaultFixture]
        public async void GetAsync_ShouldReturnTimeoutExceptionViaResults_WhenTimeoutIsSpecified_AndTaskIsSlower(DummyData dummyData)
        {
            async Task<DummyData> TaskFactory()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                return dummyData;
            }
            var sut = new DataCollector<DummyData>(TaskFactory, null, TimeSpan.FromMilliseconds(10));

            var actual = await sut.GetAsync();

            actual.Data.Should().BeNull();
            actual.Error.GetType().Should().Be(typeof(TimeoutException));
        }
        
        [NamedTheory, DefaultFixture]
        public async void GetAsync_ShouldNotReturnTimeoutException_WhenTimeoutIsSpecified_AndTaskIsFaster(DummyData dummyData)
        {
            async Task<DummyData> TaskFactory()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                return dummyData;
            }
            var sut = new DataCollector<DummyData>(TaskFactory, null, TimeSpan.FromMilliseconds(100));

            var actual = await sut.GetAsync();
            
            actual.Data.Should().NotBeNull();
            actual.Error.Should().BeNull();
        }
        
        [NamedFact]
        public async void GetAsync_ShouldReturnExceptionViaResults_WhenTaskFactoryThrows()
        {
            var expected = new NullReferenceException();
            Task<DummyData> TaskFactory()
            {
                throw expected;
            }
            var sut = new DataCollector<DummyData>(TaskFactory);

            var actual = await sut.GetAsync();
            
            actual.Data.Should().BeNull();
            actual.Error.Should().Be(expected);
        }

        #region Thread-safety tests

        [NamedFact]
        public void GetAsync_ShouldOnlyCallTaskOnce_WhenCalledFrom20ParallelThreads_AndTaskIsSlow()
        {
            var dummyData = new DummyData();
            var calls = 0;
            async Task<DummyData> TaskFactory()
            {
                Interlocked.Increment(ref calls);
                await Task.Delay(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                return dummyData;
            }
            var sut = new DataCollector<DummyData>(TaskFactory);

            const int count = 20;
            var threads = new Thread[count];
            for (var i = 0; i < count; i++)
            {
                threads[i] = new Thread(async () => { await sut.GetAsync().ConfigureAwait(false); });
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Start();
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Join();
            }

            calls.Should().BeLessOrEqualTo(2);
        }

        [NamedFact]
        public void GetAsync_ShouldCallTaskMoreThanOnce_WhenCalledFrom20ParallelThreads_AndTaskIsFast()
        {
            var dummyData = new DummyData();
            var calls = 0;
            Task<DummyData> TaskFactory()
            {
                Interlocked.Increment(ref calls);
                return Task.FromResult<DummyData>(dummyData);
            }
            var sut = new DataCollector<DummyData>(TaskFactory);

            const int count = 20;
            var threads = new Thread[count];
            for (var i = 0; i < count; i++)
            {
                threads[i] = new Thread(async () => await sut.GetAsync().ConfigureAwait(false));
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Start();
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Join();
            }

            calls.Should().BeGreaterThan(1);
        }

        [NamedFact]
        public void GetAsync_ShouldCallTaskOnlyOnce_WhenCalledFrom20ParallelThreads_AndTaskIsFast_AndHasCacheDuration()
        {
            var dummyData = new DummyData();
            var calls = 0;
            Task<DummyData> TaskFactory()
            {
                Interlocked.Increment(ref calls);
                return Task.FromResult<DummyData>(dummyData);
            }
            var sut = new DataCollector<DummyData>(TaskFactory, TimeSpan.FromSeconds(60));

            const int count = 20;
            var threads = new Thread[count];
            for (var i = 0; i < count; i++)
            {
                threads[i] = new Thread(async () => await sut.GetAsync().ConfigureAwait(false));
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Start();
            }
            for (var i = 0; i < count; i++)
            {
                threads[i].Join();
            }

            calls.Should().Be(1);
        }

        #endregion

        [NamedTheory, DefaultFixture]
        public void Equals_ShouldReturnTrue_WhenTypesAreSameButInstancesAreDifferent(TimeSpan cacheDuration, Func<Task<DummyData>> taskFactory)
        {
            var sut1 = new DataCollector<DummyData>(taskFactory, cacheDuration);
            var sut2 = new DataCollector<DummyData>(taskFactory, cacheDuration);

            // ReSharper disable | Testing the full IEquatable here, so leave this explicit
            (sut1.Equals(sut2)).Should().BeTrue();
            (sut1 == sut2).Should().BeTrue();
            (sut1 != sut2).Should().BeFalse();
            (object.Equals(sut1, sut2)).Should().BeTrue();
            // ReSharper enable
        }

        [NamedTheory, DefaultFixture]
        public void Equals_ShouldReturnTrue_WhenTypesAreSameButEverythingElseIsDifferent(TimeSpan cacheDuration1, Func<Task<DummyData>> taskFactory1, TimeSpan cacheDuration2, Func<Task<DummyData>> taskFactory2)
        {
            var sut1 = new DataCollector<DummyData>(taskFactory1, cacheDuration1);
            var sut2 = new DataCollector<DummyData>(taskFactory2, cacheDuration2);

            // ReSharper disable | Testing the full IEquatable here, so leave this explicit
            (sut1.Equals(sut2)).Should().BeTrue();
            (object.Equals(sut1, sut2)).Should().BeTrue();
            // ReSharper enable
        }

        [NamedTheory, DefaultFixture]
        public void Equals_ShouldReturnFalse_WhenTypesAreDifferent(TimeSpan cacheDuration, Func<Task<DummyData>> taskFactory1, Func<Task<DummyData2>> taskFactory2)
        {
            var sut1 = new DataCollector<DummyData>(taskFactory1, cacheDuration);
            var sut2 = new DataCollector<DummyData2>(taskFactory2, cacheDuration);

            // ReSharper disable | Testing the full IEquatable here, so leave this explicit
            (sut1.Equals(sut2)).Should().BeFalse();
            (object.Equals(sut1, sut2)).Should().BeFalse();
            // ReSharper enable
        }

        [NamedTheory, DefaultFixture]
        public void Equals_ShouldReturnFalse_WhenIdentifiersAreDifferent(TimeSpan cacheDuration, Func<Task<DummyData>> taskFactory, Identity id)
        {
            var sut1 = new DataCollector<DummyData>(taskFactory, cacheDuration);
            var sut2 = new DataCollector<DummyData>(taskFactory, id, cacheDuration);

            // ReSharper disable | Testing the full IEquatable here, so leave this explicit
            (sut1.Equals(sut2)).Should().BeFalse();
            (sut1 == sut2).Should().BeFalse();
            (sut1 != sut2).Should().BeTrue();
            (object.Equals(sut1, sut2)).Should().BeFalse();
            // ReSharper enable
        }
    }

    public sealed class DummyData
    {

    }

    public sealed class DummyData2
    {

    }
}
