using System;
using System.Net.Http;
using System.Web;
using Nimator.Logging;

namespace Nimator.Web.Util
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Reminder: only use when constructor-, property- and method injection are not an option!
        /// </summary>
        public static object ResolveFromRequestScope(this HttpContextBase httpContext, Type type)
        {
            var request = httpContext.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            var scope = request.GetDependencyScope();
            return scope.GetService(type);
        }

        /// <summary>
        /// Reminder: only use when constructor-, property- and method injection are not an option!
        /// </summary>
        public static object ResolveFromRequestScope(this HttpContext httpContext, Type type)
        {
            var request = httpContext.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            var scope = request.GetDependencyScope();
            return scope.GetService(type);
        }

        private static object ResolveFromRequestScope(dynamic httpContext, Type type)
        {
            var request = httpContext.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
            var scope = request.GetDependencyScope();
            return scope.GetService(type);
        }

        public static ILog GetLogger(this HttpContext httpContext)
        {
            return ResolveFromRequestScope(httpContext, typeof(ILog)) as ILog;
        }
    }
}
