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
                .CreateLogger();

            var builder = CreateHostBuilder(args);

            builder.RunAsTopshelfService(hc =>
            {
                hc.SetServiceName("GenericHostInTopshelf");
                hc.SetDisplayName("Generic Host In Topshelf");
                hc.SetDescription("Runs a generic host as a Topshelf service.");
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
            return new ClockService(900000); // 900000 is the number of milliseconds in 15 minutes
        }
    }
}
