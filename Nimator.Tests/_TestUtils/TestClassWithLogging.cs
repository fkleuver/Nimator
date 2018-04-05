using Nimator.Logging;
using Xunit.Abstractions;

namespace Nimator.Tests
{
    /// <summary>
    /// Base test class that configures LibLog to write to the Xunit test output.
    /// Must pass <see cref="ITestOutputHelper"/> to the base constructor.
    /// </summary>
    public abstract class TestClassWithLogging
    {
        protected TestClassWithLogging(ITestOutputHelper output)
        {
            LogProvider.SetCurrentLogProvider(new TestOutputLogProvider(output));
        }
    }
}
