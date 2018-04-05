using System.Linq;
using Castle.DynamicProxy;
using Nimator.Logging;
using Nimator.Util;
// ReSharper disable ClassNeverInstantiated.Global

namespace Nimator.Web.Util
{

    /// <summary>
    /// A logging interceptor which can be applied to classes at runtime.
    /// It will intercept each of its methods and perform some logging before & after the method has been invoked.
    /// </summary>
    public sealed class CallLogInterceptor : IInterceptor
    {
        private readonly ILog _logger;

        public CallLogInterceptor(ILog logger)
        {
            Guard.AgainstNull(nameof(logger), logger);

            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            Log($"Calling method {invocation.Method.Name} with parameters {string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())}... ");

            invocation.Proceed();

            Log($"Done: result was {invocation.ReturnValue}.");
        }

        private void Log(string message)
        {
            _logger.Debug($"[{nameof(CallLogInterceptor)}] {message}");
        }
    }
}
