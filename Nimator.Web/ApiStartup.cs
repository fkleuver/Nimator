using System;
using System.Web.Http;
using Autofac;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Logging;
using Nimator.CouchBase;
using Nimator.CouchBase.Rules;
using Nimator.Logging;
using Nimator.Notifiers;
using Nimator.Util;
using Nimator.Web.Hubs;
using Nimator.Web.Util;
using Owin;

namespace Nimator.Web
{
    public sealed class ApiStartup
    {
        private ILog _logger;

        public void Configuration(IAppBuilder app)
        {
            _logger = LogProvider.GetLogger("LibLog");

            try
            {
                var settings = AppSettings.FromConfigurationManager();
                var config = new HttpConfiguration();
                var builder = new ContainerBuilder();
                config.ConfigureErrorHandling(_logger);
                builder.RegisterModule(new ApiModule(config));
                var container = builder.Build();

                app.SetLoggerFactory(new LibLogLoggerFactory());
                app.UseCorsWithExposedHeaders(origins: new[] { settings.ClientBaseUri });
                app.UseAutofacWebApiStack(container, config);

                app.Map("/signalr", map =>
                {
                    var hubConfig = new HubConfiguration();
                    map.UseCors(CorsOptions.AllowAll);
                    map.RunSignalR(hubConfig);
                });
                SignalRContractResolver.Configure();

                ConfigureHealthMonitor();

                _logger.Info($"App is available at {settings.ApiBaseUri}");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error during Startup.Configuration", ex);
                throw;
            }
        }

        private static void ConfigureHealthMonitor()
        {

            // Create healthcheck that uses a ClusterManager based on app.config username/password
            // This will cause monitoring failure alerts if no valid config is present
            var couchBaseCheck = new ClusterHealthCheck(ClusterManagerFactory.FromAppSettings(AppSettings.FromConfigurationManager()));

            // intentionally using extreme values here to make sure the alerts go off :)
            couchBaseCheck 
                .AddRule(new MaxDiskReads(0))
                .AddRule(new MaxDiskUsage(0))
                .AddRule(new MaxTotalDocumentsInBucket(5))
                .AddRule(new MinAvailablePoolMemoryQuotaPercentage(98));

            HealthMonitor.AddCheck(couchBaseCheck);

            // uncomment to spam SignalR
            //HealthMonitor.AddCheck(new HealthCheck("Spam", 2)
            //    .AddDataCollector(() => Task.FromResult("spam"))
            //    .AddRule<string>(rule => rule.WhenResult(r => true, (health, data) => health.SetLevel(LogLevel.Error).SetReason("Spam!"))));
            
            HealthMonitor.AddNotifier(ConsoleNotifierSettings.Create().ToNotifier());
            HealthMonitor.AddNotifier(SignalRNotifierSettings.Create().ToNotifier());
            HealthMonitor.StartTicking();
        }
    }
}
