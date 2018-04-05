using System;
using FluentAssertions;

namespace Nimator.Tests
{
    public class DataCollectionResultTests
    {
        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldThrow_WhenOriginIsNull(long start, long end, object data)
        {
            Action act = () => new DataCollectionResult<object>(null, start, end, data);

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldThrow_WhenDataIsNull(IDataCollector origin, long start, long end)
        {
            Action act = () => new DataCollectionResult<object>(origin, start, end, (object)null);

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldThrow_WhenExceptionIsNull(IDataCollector origin, long start, long end)
        {
            Action act = () => new DataCollectionResult<object>(origin, start, end, (Exception)null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
