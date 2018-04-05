using System;
using FluentAssertions;
using Nimator.Formatters;
using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class SlackNotifierTests
    {
        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldThrow_WhenSettingsIsNull(IHealthCheckResultFormatter formatter)
        {
            Action act = () => new SlackNotifier(null, formatter);

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldNotThrow_WhenFormatterIsNull(SlackNotifierSettings settings)
        {
             new SlackNotifier(settings);
        }
    }
}
