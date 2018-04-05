using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Nimator.Logging;

namespace Nimator.Web.Util
{
    public sealed class LogProviderExceptionLogger : IExceptionLogger
    {
        private readonly ILog _logger;
        
        public LogProviderExceptionLogger(ILog logger)
        {
            _logger = logger;
        }

        public async Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            await Task.Run(() => _logger.ErrorException("Unhandled exception", context.Exception), cancellationToken);
        }
    }
}
