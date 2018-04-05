using System;
using FluentAssertions;
using Nimator.Formatters;
using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class OpsGenieNotifierTests
    {
        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldThrow_WhenSettingsIsNull(IHealthCheckResultFormatter formatter)
        {
            Action act = () => new OpsGenieNotifier(null, formatter);

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldNotThrow_WhenFormatterIsNull(OpsGenieNotifierSettings settings)
        {
            new OpsGenieNotifier(settings, null);
        }
    }
}
