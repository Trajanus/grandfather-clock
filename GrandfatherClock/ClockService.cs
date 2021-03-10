using Microsoft.Extensions.Hosting;
using NAudio.Wave;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace GrandfatherClock
{
    public class ClockService : IHostedService
    {
        private const int MillisecondsInHour = 3600000;
        private const int MillisecondsInFiveMinutes = 300000;

        private readonly string _chimeFolderPath;
        private readonly int _millisecondsFactor;
        private readonly Timer _timer;

        private RawSourceWaveStream _chime;

        public ClockService(int chimeIntervalInMilliseconds)
        {
            _millisecondsFactor = chimeIntervalInMilliseconds;
            _timer = new Timer(GetMillisecondsToNextChime(_millisecondsFactor)) { AutoReset = true };
            Log.Information($"Next chime will be at {DateTime.Now.AddMilliseconds(_timer.Interval)}");
            _timer.Elapsed += Chime;
            _chimeFolderPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\GrandfatherClock\chimes\";
        }
        public async Task StartAsync(System.Threading.CancellationToken cancellationToken) 
        {
            if (!Directory.Exists(_chimeFolderPath))
            {
                Log.Warning($"Chime folder {_chimeFolderPath} does not exist, it will be created.");
                Directory.CreateDirectory(_chimeFolderPath);
            }

            try
            {
                _chime = await LoadChime($"{_chimeFolderPath}chime.mp3");
            }
            catch (FileNotFoundException ex)
            {
                Log.Error($"Failed to find chime file at {_chimeFolderPath}chime.mp3", ex);
            }

            _timer.Start(); 
            return; 
        }
        public Task StopAsync(System.Threading.CancellationToken cancellationToken) 
        { 
            _timer.Stop(); 
            Log.CloseAndFlush(); 
            return Task.CompletedTask; 
        }

        private async Task<RawSourceWaveStream> LoadChime(string chimeFilePath)
        {
            RawSourceWaveStream chime;
            Mp3WaveFormat format;
            MemoryStream stream = new MemoryStream();

            using (var audioFile = File.OpenRead(chimeFilePath))
            using (var mp3 = new Mp3FileReader(audioFile))
            {
                format = mp3.Mp3WaveFormat;
                await audioFile.CopyToAsync(stream);
            }

            chime = new RawSourceWaveStream(stream, format);
            chime.Position = 0;

            return chime;
        }

        private void Chime(object sender, ElapsedEventArgs eventArgs)
        {
            Timer timer = (Timer)sender;
            timer.Interval = GetMillisecondsToNextChime(_millisecondsFactor);
            Log.Information($"Next chime will be at {DateTime.Now.AddMilliseconds(timer.Interval)}");

            PlayChime(_chime);
        }

        private void PlayChime(RawSourceWaveStream chime)
        {
            if(null == chime)
            {
                Console.Beep();
            }
            else
            {
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(chime);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Task.Delay(1000);
                    }
                }
                chime.Position = 0;
            }
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
