using System;
using System.IO;
using Microsoft.Owin.Logging;
using Nimator.Logging;
using Nimator.Util;
using Nimator.Web.Middlewares;
using Nimator.Web.Util;
using Owin;

namespace Nimator.Web
{
    public class ClientStartup
    {
        private ILog _logger;

        public void Configuration(IAppBuilder app)
        {
            _logger = LogProvider.GetLogger("LibLog");

            try
            {
                var settings = AppSettings.FromConfigurationManager();
                var bootstrapperSettings = new SPABootstrapperSettings("/.spa/bootstrap")
                    .WithInformation(nameof(settings.ApiBaseUri), settings.ApiBaseUri)
                    .WithInformation(nameof(settings.ClientBaseUri), settings.ClientBaseUri);

                app.SetLoggerFactory(new LibLogLoggerFactory());
                app.UseSPABootstrapper(bootstrapperSettings);
                app.UseSPAClientFiles(GetDynamicFilesOptions(_logger));

                _logger.Info($"App is available at {settings.ClientBaseUri}");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error during Startup.Configuration", ex);
                throw;
            }
        }

        private static SPAClientFilesOptions GetDynamicFilesOptions(ILog logger)
        {
            var localCodeBase = typeof(ClientStartup).Assembly.GetLocalCodeBase();

            var codebase = localCodeBase;
            var executionDir = codebase.Substring(0, codebase.LastIndexOf(@"\", StringComparison.Ordinal));
            var clientDir = Path.Combine(executionDir, @"wwwroot");
            var options = new SPAClientFilesOptions
            {
                BaseDirectory = @"wwwroot",
                Logger = logger,
                LogHandledRequests = true,
                LogSkippedRequests = true,
                AbortIfFileNotFound = false,
                SPARootPaths = new string[0]
            };
            return options;
        }
    }
}
