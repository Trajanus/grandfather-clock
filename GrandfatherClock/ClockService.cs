using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace FishmanIndustries
{
    public class ClockService : IHostedService
    {
        private const int MillisecondsInHour = 3600000;
        private const int MillisecondsInFiveMinutes = 300000;

        private readonly Options _options;
        private GrandfatherClock _clock;
        private readonly Timer _timer;

        public ClockService(Options options)
        {
            _options = options;
            _timer = new Timer(GetMillisecondsToNextChime(_options.ClockOptions.ChimeInterval)) { AutoReset = true };
            Log.Information($"Next chime will be at {DateTime.Now.AddMilliseconds(_timer.Interval)}");
            _timer.Elapsed += Chime;
        }

        public async Task StartAsync(System.Threading.CancellationToken cancellationToken) 
        {
            if (!Directory.Exists(_options.ChimesDirectoryPath))
            {
                Log.Warning($"Chime folder {_options.ChimesDirectoryPath} does not exist, it will be created.");
                Directory.CreateDirectory(_options.ChimesDirectoryPath);
            }

            RawSourceWaveStream chimeIntro = await LoadChime($"{_options.ChimesDirectoryPath}chime-intro.wav");
            RawSourceWaveStream chime = await LoadChime($"{_options.ChimesDirectoryPath}chime.wav");
            RawSourceWaveStream finalChime = await LoadChime($"{_options.ChimesDirectoryPath}final-chime.wav");

            _clock = new GrandfatherClock(_options.ClockOptions, chimeIntro, chime, finalChime);

            _timer.Start(); 
            return; 
        }
        public Task StopAsync(System.Threading.CancellationToken cancellationToken) 
        { 
            _timer.Stop(); 
            Log.CloseAndFlush(); 
            return Task.CompletedTask; 
        }

        private void Chime(object sender, ElapsedEventArgs eventArgs)
        {
            try
            {
                Timer timer = (Timer)sender;
                timer.Interval = GetMillisecondsToNextChime(_options.ClockOptions.ChimeInterval);
                Log.Information($"Next chime will be at {DateTime.Now.AddMilliseconds(timer.Interval)}");

                _clock.PlayChime();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        private async Task<RawSourceWaveStream> LoadChime(string chimeFilePath)
        {
            RawSourceWaveStream chime = null;
            WaveFormat format;
            MemoryStream stream = new MemoryStream();

            try
            {
                using (var audioFile = File.OpenRead(chimeFilePath))
                using (var waveReader = new WaveFileReader(audioFile))
                {
                    format = waveReader.WaveFormat;
                    await audioFile.CopyToAsync(stream);
                }

                chime = new RawSourceWaveStream(stream, format);
                chime.Position = 0;

            }
            catch (FileNotFoundException ex)
            {
                Log.Error($"Failed to find chime file at {_options.ChimesDirectoryPath}chime.wav", ex);
            }

            return chime;
        }

        private int GetMillisecondsToNextChime(int millisecondsFactor)
        {
            if (millisecondsFactor > MillisecondsInHour)
                millisecondsFactor = MillisecondsInHour;

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
