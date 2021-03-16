using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Topshelf.Extensions.Hosting;

namespace GrandfatherClock
{
    class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(@"C:\Logs\grandfather-clock\log-.txt", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();

            var builder = CreateHostBuilder(args);

            builder.RunAsTopshelfService(hc =>
            {
                hc.SetServiceName("GrandfatherClock");
                hc.SetDisplayName("Grandfather Clock");
                hc.SetDescription("Chimes every hour.");
            });
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService(ClockServiceFactory);
                });
        }

        public static ClockService ClockServiceFactory(System.IServiceProvider provider)
        {
            return new ClockService(3600000); // 3600000 is the number of milliseconds in 1 hour
        }
    }
}
