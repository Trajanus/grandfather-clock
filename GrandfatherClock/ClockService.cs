using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace GrandfatherClock
{
    public class ClockService : IHostedService
    {
        readonly Timer _timer;
        public ClockService()
        {
            _timer = new Timer(1000) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) => Console.WriteLine("It is {0} and all is well", DateTime.Now);
        }
        public Task StartAsync(System.Threading.CancellationToken cancellationToken) { _timer.Start(); return Task.CompletedTask; }
        public Task StopAsync(System.Threading.CancellationToken cancellationToken) { _timer.Stop(); return Task.CompletedTask; }
    }
}
