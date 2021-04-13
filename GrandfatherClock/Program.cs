﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Topshelf.Extensions.Hosting;

namespace FishmanIndustries
{
    class Program
    {
        public static Options options;

        public static void Main(string[] args)
        {
            options = ReadConfiguration();

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
            return new ClockService(options.ClockOptions);
        }

        public static Options ReadConfiguration()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");
            IConfiguration configuration = configurationBuilder.Build();
            var options = new Options();
            configuration.Bind(options);
            return options;
        }
    }
}
