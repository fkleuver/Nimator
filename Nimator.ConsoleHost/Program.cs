using System;
using Nimator.CouchBase;
using Nimator.CouchBase.Rules;
using Nimator.Notifiers;
using Nimator.Util;
using Serilog;

namespace Nimator.ConsoleHost
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            // Initialize the logger from app.config settings
            Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().Enrich.WithThreadId().CreateLogger();

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

            // Add a logging notifier which, by default, logs the results to console as json
            HealthMonitor.AddNotifier(LibLogNotifierSettings.Create().ToNotifier());

            // Start the ticking loop
            HealthMonitor.StartTicking();

            // "press any key to stop"
            Console.ReadKey();
        }
    }
}
