using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Owin.Hosting;
using Nimator.Logging;
using Owin;
using Serilog;
using Serilog.Events;

namespace Nimator.Web.Util
{
    /// <summary>
    /// A rough utility class for common console things
    /// </summary>
    public static class ConsoleBootstrapper
    {
        private static ILog _logger;
        private static ILog Logger => _logger ?? (_logger = LogProvider.GetCurrentClassLogger());


        private const string LogFormat =
            @"{Timestamp:HH:mm} [{Level}] ({Name:l}){NewLine} {Message}{NewLine}{Exception}";

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy,
            int wFlags);

        private static string AppName = "OWIN";
        
        public static void Run(string url, Action<IAppBuilder> startup, int? xOffset = null)
        {
            AppName = $"{AppName} - {url}";
            RunInternal(() => WebApp.Start(url, startup), xOffset);
        }
        
        public static void Run(StartOptions options, Action<IAppBuilder> startup, int? xOffset = null)
        {
            AppName = $"{AppName} - {options.Urls.FirstOrDefault() ?? $"Port {options.Port}"}";
            RunInternal(() => WebApp.Start(options, startup), xOffset);
        }
        
        public static void Run<TStartup>(string url, int? xOffset = null)
        {
            AppName = $"{AppName} - {url}";
            RunInternal(() => WebApp.Start<TStartup>(url), xOffset);
        }
        
        public static void Run<TStartup>(StartOptions options, int? xOffset = null)
        {
            AppName = $"{AppName} - {options.Urls.FirstOrDefault() ?? $"Port {options.Port}"}";
            RunInternal(() => WebApp.Start<TStartup>(options), xOffset);
        }
        
        public static void Run(string url, int? xOffset = null)
        {
            AppName = $"{AppName} - {url}";
            RunInternal(() => WebApp.Start(url), xOffset);
        }
        
        public static void Run(StartOptions options, int? xOffset = null)
        {
            AppName = $"{AppName} - {options.Urls.FirstOrDefault() ?? $"Port {options.Port}"}";
            RunInternal(() => WebApp.Start(options), xOffset);
        }

        private static void RunInternal(Func<IDisposable> startAction, int? xOffset = null)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .Console(outputTemplate: LogFormat)
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .CreateLogger();

            Logger.Info($"Starting application: {AppName}");

            SetWindowPos(
                hWnd: GetConsoleWindow(),
                hWndInsertAfter: 0,
                x: xOffset ?? 0,
                y: 0,
                cx: 0,
                cy: 0,
                wFlags: 0);

            Console.Title = AppName;
            Console.CursorVisible = false;
            Console.SetWindowSize(80, 60);

            IDisposable server = null;
            try
            {
                server = startAction();
                PromptForExit(server);
            }
            catch (Exception ex)
            {
                var exception = ex;
                while (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                }
                Logger.FatalException(@"Unable to start application", exception);
                PromptForExit(server);
            }
        }

        private static void PromptForExit(IDisposable server)
        {
            Logger.Info(@"Press [Enter] to exit application");
            Console.ReadLine();
            Logger.Info(@"Shutting down..");
            Thread.Sleep(1000);
            try
            {
                server?.Dispose();
            }
            catch
            {
                // Ignored
            }
            finally
            {
                Environment.Exit(-1);
            }
        }
    }
}
