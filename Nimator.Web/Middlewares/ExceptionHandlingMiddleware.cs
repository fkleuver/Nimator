using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.Web.Middlewares
{
    public sealed class ExceptionHandlingMiddleware : OwinMiddleware
    {
        private readonly ILog _logger;

        public ExceptionHandlingMiddleware(OwinMiddleware next, ILog logger) : base(next)
        {
            _logger = logger;
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
            }
            catch (HttpRequestException)
            {
                if ((context.Request.Body is TimeoutStream requestTimeoutStream && requestTimeoutStream.TimedOut) ||
                    (context.Response.Body is TimeoutStream responseTimeoutStream && responseTimeoutStream.TimedOut))
                {
                    context.Response.StatusCode = 503;

                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("An error occurred in the OWIN pipeline", ex);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(@"{""Message"":""An error occurred in the OWIN pipeline. Further information can be found in the logs.""}");
            }
        }
    }
}
