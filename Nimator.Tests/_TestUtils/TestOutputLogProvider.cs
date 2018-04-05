using System;
using System.Text;
using Nimator.Logging;
using Xunit.Abstractions;

namespace Nimator.Tests
{
    /// <summary>
    /// An implementation of LibLog's <see cref="ILogProvider"/> that, when registered through "LogProvider.SetCurrentLogProvider", directs all logging output to XUnit's <see cref="ITestOutputHelper"/>
    /// </summary>
    /// <remarks>
    /// See also:
    /// - https://xunit.github.io/docs/capturing-output.html
    /// - https://github.com/damianh/LibLog/wiki
    /// </remarks>
    public sealed class TestOutputLogProvider : ILogProvider
    {
        private readonly ITestOutputHelper _output;

        public TestOutputLogProvider(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// An inline implementation for LibLog's <see cref="ILog"/> interface.
        /// </summary>
        private bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
        {
            var output = _output;
            var sb = new StringBuilder();
            if (messageFunc != null)
            {
                sb.Append($"[{logLevel}]: ");
                if (formatParameters.Length > 0)
                {
                    sb.AppendFormat(messageFunc(), formatParameters);
                }
                else
                {
                    sb.Append(messageFunc());
                }

                if (exception != null)
                {
                    sb.Append(exception.Message);
                }

            }
            output.WriteLine(sb.ToString());
            return true;
        }


        public Logger GetLogger(string name)
        {
            return Log;
        }

        public IDisposable OpenNestedContext(string message)
        {
            return null;
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            return null;
        }
    }
}
