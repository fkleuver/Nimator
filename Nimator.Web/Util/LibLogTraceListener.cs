using System;
using System.Diagnostics;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.Web.Util
{
    public sealed class LibLogTraceListener : TraceListener
    {
        private readonly ILog _logger;

        private LibLogTraceListener(ILog logger)
        {
            _logger = logger;
        }

        public static LibLogTraceListener CreateUsingLogger(ILog logger)
        {
            Guard.AgainstNull(nameof(logger), logger);

            return new LibLogTraceListener(logger);
        }

        public static LibLogTraceListener CreateUsingLogger(string name)
        {
            var logger = LogProvider.GetLogger(name);

            return CreateUsingLogger(logger);
        }

        public static LibLogTraceListener CreateUsingLogger(Type type, string fallbackTypeName = "System.Object")
        {
            var logger = LogProvider.GetLogger(type, fallbackTypeName);

            return CreateUsingLogger(logger);
        }

        public static LibLogTraceListener CreateUsingCurrentClassLogger()
        {
            var logger = LogProvider.GetCurrentClassLogger();

            return CreateUsingLogger(logger);
        }

        public override void WriteLine(string message)
        {
            _logger.Debug(message);
        }

        public override void Write(string message)
        { }
    }
}
