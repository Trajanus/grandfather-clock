using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Topshelf.Extensions.Hosting;

namespace GrandfatherClock
{
    class Program
    {
        public static GrandfatherClockOptions options;

        public static void Main(string[] args)
        {
            GrandfatherClockOptions options = ReadConfiguration();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(options.LogDirectoryPath, rollingInterval: RollingInterval.Day)
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
            return new ClockService(3600000, options); // 3600000 is the number of milliseconds in 1 hour
        }

        public static GrandfatherClockOptions ReadConfiguration()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            IConfiguration configuration = configurationBuilder.Build();
            options = new GrandfatherClockOptions();
            configuration.Bind(options);
            return options;
        }
    }
}
