using Microsoft.Owin.Hosting;
using Nimator.Util;
using Nimator.Web.Util;
using Serilog;

namespace Nimator.Web.Client.ConsoleHost
{

    internal sealed class Program
    {
        internal static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().Enrich.WithThreadId().CreateLogger();
            var appSettings = AppSettings.FromConfigurationManager();
            ConsoleBootstrapper.Run<ClientStartup>(new StartOptions { Urls = { appSettings.ClientBaseUri } }, 500);
        }
    }
}
