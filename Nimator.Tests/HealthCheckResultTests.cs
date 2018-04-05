using System;
using FluentAssertions;
using Nimator.Logging;

namespace Nimator.Tests
{
    public class HealthCheckResultTests
    {
        [NamedFact]
        public void Constructor_ShouldThrow_WhenIdentityIsNull()
        {
            Action act = () => new HealthCheckResult(null);

            act.Should().Throw<ArgumentNullException>();
        }

        // More of a smoke test than a unit test, but it should do the trick for now
        [NamedFact]
        public void Finalize_ShouldCorrectlyBubbleUpInnerResults()
        {
            var sut = HealthCheckResult
                .Create("Foo")
                .SetStatus(Status.Okay)
                .SetLevel(LogLevel.Info)
                .AddInnerResult(HealthCheckResult
                    .Create("Foo1")
                    .SetStatus(Status.Okay)
                    .SetLevel(LogLevel.Info)
                    .AddInnerResult(HealthCheckResult
                        .Create("Bar1")
                        .SetStatus(Status.Warning)
                        .SetLevel(LogLevel.Warn)
                        .SetReason("Bar1!"))
                    .AddInnerResult(HealthCheckResult
                        .Create("Bar2")
                        .SetStatus(Status.Warning)
                        .SetLevel(LogLevel.Warn)
                        .SetReason("Bar2!"))
                    .AddInnerResult(HealthCheckResult
                        .Create("Bar2")
                        .SetStatus(Status.Warning)
                        .SetLevel(LogLevel.Warn)
                        .SetReason("Bar2!"))
                    .AddInnerResult(HealthCheckResult
                        .Create("Bar2")
                        .SetStatus(Status.Warning)
                        .SetLevel(LogLevel.Warn)
                        .SetReason("Bar3!")))
                .AddInnerResult(HealthCheckResult
                    .Create("Foo2")
                    .SetStatus(Status.Okay)
                    .SetLevel(LogLevel.Info)
                    .AddInnerResult(HealthCheckResult
                        .Create("Baz1")
                        .SetStatus(Status.Critical)
                        .SetLevel(LogLevel.Error)
                        .SetReason("Baz1!"))
                    .AddInnerResult(HealthCheckResult
                        .Create("Baz2")
                        .SetStatus(Status.Critical)
                        .SetLevel(LogLevel.Error)
                        .SetReason("Baz2!")))
                .AddInnerResult(HealthCheckResult
                    .Create("Foo3")
                    .SetStatus(Status.Okay)
                    .SetLevel(LogLevel.Info)
                    .AddInnerResult(HealthCheckResult
                        .Create("Qux1")
                        .SetStatus(Status.Critical)
                        .SetLevel(LogLevel.Fatal)
                        .SetReason("Qux1!"))
                    .AddInnerResult(HealthCheckResult
                        .Create("Qux2")
                        .SetStatus(Status.Critical)
                        .SetLevel(LogLevel.Fatal)
                        .SetReason("Qux2!")));

            // everything
            sut.Finalize(sut.CheckId, x => true);
            
            sut.Level.Should().Be(LogLevel.Fatal);
            sut.Status.Should().Be(Status.Critical);
            sut.Details.Keys.Count.Should().Be(7);
            sut.Details["Foo.Foo1.Bar1.Reason"].Should().Be("Bar1!");
            sut.Details["Foo.Foo1.Bar2.Reason"].Should().Be("Bar2!");
            sut.Details["Foo.Foo1.Bar2.Reason#1"].Should().Be("Bar3!");
            sut.Details["Foo.Foo2.Baz1.Reason"].Should().Be("Baz1!");
            sut.Details["Foo.Foo2.Baz2.Reason"].Should().Be("Baz2!");
            sut.Details["Foo.Foo3.Qux1.Reason"].Should().Be("Qux1!");
            sut.Details["Foo.Foo3.Qux2.Reason"].Should().Be("Qux2!");
        }
    }
}
