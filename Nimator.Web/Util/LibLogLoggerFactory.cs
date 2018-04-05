using System;
using System.Diagnostics;
using Microsoft.Owin.Logging;
using Nimator.Logging;

namespace Nimator.Web.Util
{
    public class LibLogLoggerFactory : ILoggerFactory
    {
        public ILogger Create(string name)
        {
            var libLogLogger = LogProvider.GetLogger(name);
            return new LibLogOwinLogAdapter(libLogLogger);
        }
    }

    public class LibLogOwinLogAdapter : ILogger
    {
        private readonly ILog _logger;

        public LibLogOwinLogAdapter(ILog logger)
        {
            _logger = logger;
        }

        public bool WriteCore(TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            LogLevel logLevel;
            switch (eventType)
            {
                case TraceEventType.Critical:
                    logLevel = LogLevel.Fatal;
                    break;
                case TraceEventType.Error:
                    logLevel = LogLevel.Error;
                    break;
                case TraceEventType.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case TraceEventType.Information:
                    logLevel = LogLevel.Info;
                    break;
                case TraceEventType.Verbose:
                case TraceEventType.Start:
                case TraceEventType.Stop:
                case TraceEventType.Suspend:
                case TraceEventType.Resume:
                case TraceEventType.Transfer:
                    logLevel = LogLevel.Debug;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
            }
            return _logger.Log(logLevel, () => formatter(state, exception), exception);
        }
    }
}
