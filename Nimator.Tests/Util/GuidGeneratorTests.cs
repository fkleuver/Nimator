using System;
using System.Linq;
using FluentAssertions;
using Nimator.Util;

namespace Nimator.Tests.Util
{
    /// <summary>
    /// Merely a smoke test for the core problem it solves; coverage needs to be improved
    /// </summary>
    public class GuidGeneratorTests
    {
        [NamedFact]
        public void GenerateTimeBasedGuid_ShouldNotHaveAnyCollisions_WhenCalled1000TimesInARow()
        {
            const int count = 1000;
            var guids = new Guid[count];
            for (int i = 0; i < count; i++)
            {
                guids[i] = GuidGenerator.GenerateTimeBasedGuid();
            }

            guids.Length.Should().Be(guids.Distinct().Count());
        }
    }
}
