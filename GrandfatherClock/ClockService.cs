using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace GrandfatherClock
{
    public class ClockService : IHostedService
    {
        private const int MillisecondsInHour = 3600000;
        private const int MillisecondsInFiveMinutes = 300000;
        private readonly int _millisecondsFactor;
        private readonly Timer _timer;
        public ClockService(int chimeIntervalInMilliseconds)
        {
            _millisecondsFactor = chimeIntervalInMilliseconds;
            _timer = new Timer(GetMillisecondsToNextChime(_millisecondsFactor)) { AutoReset = true };
            Log.Information($"Next chime will be at {DateTime.Now.AddMilliseconds(_timer.Interval)}");
            _timer.Elapsed += Chime;
        }
        public Task StartAsync(System.Threading.CancellationToken cancellationToken) { _timer.Start(); return Task.CompletedTask; }
        public Task StopAsync(System.Threading.CancellationToken cancellationToken) 
        { 
            _timer.Stop(); 
            Log.CloseAndFlush(); 
            return Task.CompletedTask; 
        }

        private void Chime(object sender, ElapsedEventArgs eventArgs)
        {
            Timer timer = (Timer)sender;
            timer.Interval = GetMillisecondsToNextChime(_millisecondsFactor);
            Log.Information($"Next chime will be at {DateTime.Now.AddMilliseconds(timer.Interval)}");
            Console.Beep();
        }

        private int GetMillisecondsToNextChime(int millisecondsFactor)
        {
            if (millisecondsFactor > MillisecondsInHour)
                millisecondsFactor = MillisecondsInHour;

            if (millisecondsFactor < MillisecondsInFiveMinutes)
                millisecondsFactor = MillisecondsInFiveMinutes;

            int elapsedMillisecondsInCurrentHour = (DateTime.Now.Minute * 60 * 1000) + (DateTime.Now.Second * 1000) + DateTime.Now.Millisecond;
            int factor = millisecondsFactor;
            int nearestMultiple =
                    (int)Math.Ceiling(
                         (elapsedMillisecondsInCurrentHour / (double)factor)
                     ) * factor;

            return nearestMultiple - elapsedMillisecondsInCurrentHour;
        }
    }
}
