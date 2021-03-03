using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Topshelf.Extensions.Hosting;

namespace GrandfatherClock
{
    class Program
    {
        public static void Main(string[] args)
        {
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
                    services.AddHostedService<ClockService>();
                });
        }
    }
}
