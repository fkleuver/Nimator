﻿using FluentAssertions;
using Nimator.Notifiers;

namespace Nimator.Tests.Notifiers
{
    public class ConsoleNotifierTests
    {
        [NamedFact]
        public void Constructor_ShouldHaveCorrectGuardClauses()
        {
            typeof(ConsoleNotifier).VerifyConstructorGuards().Should().Be(1);
        }

        [NamedFact]
        public void InstanceMethods_ShouldHaveCorrectGuardClauses()
        {
            var sut = ConsoleNotifierSettings.Create().ToNotifier();
            typeof(ConsoleNotifier).VerifyInstanceMethodGuards(sut).Should().Be(1);
        }
    }
}
