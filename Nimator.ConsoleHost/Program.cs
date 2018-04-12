using System;
using Nimator.Notifiers;
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
            HealthMonitor.AddCheck(new BucketsHealthCheck());
            HealthMonitor.AddCheck(new ClusterHealthCheck());

            HealthMonitor.AddCheck(new PingHealthCheck());

            // Add a logging notifier which, by default, logs the results to console as json
            HealthMonitor.AddNotifier(LibLogNotifierSettings.Create().ToNotifier());

            // Start the ticking loop
            HealthMonitor.StartTicking();

            // "press any key to stop"
            Console.ReadKey();
        }
    }
}
