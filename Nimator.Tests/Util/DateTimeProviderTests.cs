using System.Linq;
using FluentAssertions;
using Nimator.Util;

namespace Nimator.Tests.Util
{
    /// <summary>
    /// Merely a smoke test for the core problem it solves; coverage needs to be improved
    /// </summary>
    public class DateTimeProviderTests
    {
        /// <summary>
        /// This will be only be true on Windows 8 or higher (that's where the API was introduced)
        /// </summary>
        [NamedFact]
        public void IsAvailable_ShouldBeTrue()
        {
            DateTimeProvider.IsSystemTimePreciseAvailable.Should().BeTrue();
        }

        /// <summary>
        /// We're verifying that DateTime.UtcNow has a low precision to make sure that the FileTime api's
        /// perceived precision is not a false positive
        /// </summary>
        [NamedFact]
        public void GetSystemTimeTicks_ShouldHaveAtLeast50PercentCollisions_WhenCalled1000TimesInARow()
        {
            const int count = 1000;
            var timestamps = new long[count];
            for (int i = 0; i < count; i++)
            {
                timestamps[i] = DateTimeProvider.GetSystemTime().Ticks;
            }

            timestamps.Length.Should().BeGreaterThan(timestamps.Distinct().Count()*2);
        }

        [NamedFact]
        public void GetSystemTimePreciseAsFileTime_ShouldHaveNoCollisions_WhenCalled1000TimesInARow()
        {
            const int count = 1000;
            var timestamps = new long[count];
            for (int i = 0; i < count; i++)
            {
                timestamps[i] = DateTimeProvider.GetSystemTimePreciseAsFileTime();
            }

            timestamps.Length.Should().Be(timestamps.Distinct().Count());
        }
    }
}
