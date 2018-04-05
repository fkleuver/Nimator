using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Nimator.Logging;
using Nimator.Web.Middlewares;
using Owin;

namespace Nimator.Web.Util
{
    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Enable HTTP logging for the middleware that comes after this one, using Autofac to resolve the logger.
        /// The Autofac middleware registration must precede this one, and the <see cref="HttpLoggingMiddleware"/> must be registered with Autofac.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IAppBuilder UseHttpLogging(this IAppBuilder app)
        {
            return app.Use<HttpLoggingMiddleware>();
        }

        /// <summary>
        /// Enable HTTP logging for the middleware that comes after this one, using the supplied logger
        /// </summary>.
        /// <param name="app"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IAppBuilder UseHttpLogging(this IAppBuilder app, ILog logger)
        {
            return app.Use<HttpLoggingMiddleware>(logger);
        }

        /// <summary>
        /// Enable exception handling for the middleware that comes after this one, using <see cref="ExceptionHandlingMiddleware"/> and using the supplied logger.
        /// </summary>.
        /// <param name="app"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IAppBuilder UseExceptionHandler(this IAppBuilder app, ILog logger)
        {
            return app.Use<ExceptionHandlingMiddleware>(logger);
        }

        /// <summary>
        /// Must come directly AFTER security middlewares such as CORS and AccessTokenValidation.
        /// Any modifications to HttpConfiguration must happen BEFORE this call, since it will be made immutable.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="container"></param>
        /// <param name="config"></param>
        /// <param name="configureSerializationWithCommonDefaults"></param>
        /// <param name="mapHttpAttributeRoutes"></param>
        /// <param name="setHttpDependencyResolverToAutofac"></param>
        /// <param name="useAutofacMiddleware"></param>
        /// <param name="useAutofacWebApi"></param>
        /// <param name="useWebApi"></param>
        /// <returns></returns>
        public static IAppBuilder UseAutofacWebApiStack(
            this IAppBuilder app,
            ILifetimeScope container,
            HttpConfiguration config,
            bool configureSerializationWithCommonDefaults = true,
            bool mapHttpAttributeRoutes = true,
            bool setHttpDependencyResolverToAutofac = true,
            bool useAutofacMiddleware = true,
            bool useAutofacWebApi = true,
            bool useWebApi = true)
        {
            if (configureSerializationWithCommonDefaults)
            {
                config.ConfigureSerializationWithCommonDefaults();
            }
            if (mapHttpAttributeRoutes)
            {
                config.MapHttpAttributeRoutes();
            }

            if (setHttpDependencyResolverToAutofac)
            {
                config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            }

            if (useAutofacMiddleware)
            {
                app.UseAutofacMiddleware(container);
            }
            if (useAutofacWebApi)
            {
                app.UseAutofacWebApi(config);
            }
            if (useWebApi)
            {
                app.UseWebApi(config);
            }

            config.EnsureInitialized();

            return app;
        }


        public static IAppBuilder UseCorsWithExposedHeaders(
            this IAppBuilder app,
            bool allowAnyHeader = true,
            bool allowAnyMethod = true,
            bool allowAnyOrigin = true,
            bool supportsCredentials = true,
            IEnumerable<string> headers = null,
            IEnumerable<string> methods = null,
            IEnumerable<string> origins = null,
            IEnumerable<string> exposedHeaders = null,
            long? preflightMaxAge = null)
        {
            var headersArr = headers?.ToArray() ?? new string[0];
            var methodsArr = methods?.ToArray() ?? new string[0];
            var originsArr = origins?.ToArray() ?? new string[0];
            var exposedHeadersArr = exposedHeaders?.ToArray() ?? new string[0];

            var policy = new CorsPolicy
            {
                AllowAnyHeader = !headersArr.Any(),
                AllowAnyMethod = !methodsArr.Any(),
                AllowAnyOrigin = !originsArr.Any(),
                SupportsCredentials = supportsCredentials,
                PreflightMaxAge = preflightMaxAge
            };

            foreach (var header in headersArr)
            {
                policy.Headers.Add(header);
            }

            foreach (var method in methodsArr)
            {
                policy.Methods.Add(method);
            }

            foreach (var origin in originsArr)
            {
                policy.Origins.Add(origin);
            }

            foreach (var exposedHeader in exposedHeadersArr)
            {
                policy.ExposedHeaders.Add(exposedHeader);
            }

            app.UseCors(new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context => Task.FromResult(policy)
                }
            });

            return app;
        }

        public static IAppBuilder UseEmbeddedFiles(this IAppBuilder app, Assembly assembly, string baseNamespace)
        {
            var fileSystem = new EmbeddedResourceFileSystem(assembly, baseNamespace);
            var fileServerOptions = new FileServerOptions
            {
                StaticFileOptions = { ServeUnknownFileTypes = true },
                FileSystem = fileSystem
            };

            app.UseFileServer(fileServerOptions);

            return app;
        }

        /// <summary>
        /// When hosting in IIS, the ExtensionlessUrlHandler needs to be bypassed. Add the following &lt;handlers&gt; element under &lt;system.webServer&gt;:
        /// &lt;add name=&quot;Owin&quot; verb=&quot;&quot; path=&quot;*&quot; type=&quot;Microsoft.Owin.Host.SystemWeb.OwinHttpHandler, Microsoft.Owin.Host.SystemWeb&quot; /&gt;
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        public static IAppBuilder UseSPAClientFiles(this IAppBuilder app, SPAClientFilesOptions options)
        {
            return app.Use<SPAClientFilesMiddleware>(options);
        }

        public static IAppBuilder UseSPABootstrapper(this IAppBuilder app, SPABootstrapperSettings settings)
        {
            return app.Use<SPABootstrapperMiddleware>(settings);
        }
    }
}
