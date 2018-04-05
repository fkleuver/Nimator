using System;
using FluentAssertions;
using Nimator.Formatters;
using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class ConsoleNotifierTests
    {
        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldThrow_WhenSettingsIsNull(IHealthCheckResultFormatter formatter, Action<string> writeLine)
        {
            Action act = () => new ConsoleNotifier(null, formatter, writeLine);

            act.Should().Throw<ArgumentNullException>();
        }

        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldNotThrow_WhenFormatterIsNull(ConsoleNotifierSettings settings, Action<string> writeLine)
        {
            new ConsoleNotifier(settings, null, writeLine);
        }

        [NamedTheory, DefaultFixture]
        public void Constructor_ShouldNotThrow_WhenWriteLineIsNull(ConsoleNotifierSettings settings, IHealthCheckResultFormatter formatter)
        {
            new ConsoleNotifier(settings, formatter);
        }
    }
}
