using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Nimator.Logging;
using Nimator.Util;

namespace Nimator.Web.Middlewares
{
    public sealed class HttpLoggingMiddleware : OwinMiddleware
    {
        private readonly ILog _logger;

        public HttpLoggingMiddleware(OwinMiddleware next, ILog logger) : base(next)
        {
            _logger = logger;
        }

        public override async Task Invoke(IOwinContext context)
        {
            await LogRequest(context.Request);

            var oldStream = context.Response.Body;
            var ms = context.Response.Body = new MemoryStream();

            try
            {
                await Next.Invoke(context);
                await LogResponse(context.Response);

                context.Response.Body = oldStream;
                await ms.CopyToAsync(oldStream);
            }
            catch (Exception ex)
            {
                _logger.DebugException("HTTP Response Exception", ex);
                throw;
            }
        }

        private async Task LogRequest(IOwinRequest request)
        {
            var reqLog = new
            {
                Method = request.Method,
                Url = request.Uri.AbsoluteUri,
                Headers = request.Headers,
                Body = await ReadBodyAsStringAsync(request)
            };

            _logger.Debug("HTTP Request" + Environment.NewLine + LogSerializer.Serialize(reqLog));
        }

        private async Task LogResponse(IOwinResponse response)
        {
            var respLog = new
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers,
                Body = await ReadBodyAsStringAsync(response)
            };

            _logger.Debug("HTTP Response" + Environment.NewLine + LogSerializer.Serialize(respLog));
        }

        private async Task<string> ReadBodyAsStringAsync(IOwinRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!request.Body.CanSeek)
            {
                var copy = new MemoryStream();
                await request.Body.CopyToAsync(copy);
                copy.Seek(0L, SeekOrigin.Begin);
                request.Body = copy;
            }

            request.Body.Seek(0L, SeekOrigin.Begin);

            string body;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 4096, true))
            {
                body = await reader.ReadToEndAsync();
            }

            request.Body.Seek(0L, SeekOrigin.Begin);

            return body;
        }

        private async Task<string> ReadBodyAsStringAsync(IOwinResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (response.Body == null)
            {
                return string.Empty;
            }

            if (response.Body.CanRead == false)
            {
                return "can't read response body";
            }

            if (!response.Body.CanSeek)
            {
                var copy = new MemoryStream();
                await response.Body.CopyToAsync(copy);
                copy.Seek(0L, SeekOrigin.Begin);
                response.Body = copy;
            }

            response.Body.Seek(0L, SeekOrigin.Begin);

            string body;
            using (var reader = new StreamReader(response.Body, Encoding.UTF8, true, 4096, true))
            {
                body = await reader.ReadToEndAsync();
            }

            response.Body.Seek(0L, SeekOrigin.Begin);

            return body;
        }
    }
}
