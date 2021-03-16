using Microsoft.Extensions.Hosting;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Serilog;
using System;
using System.Collections.Generic;
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
        private RawSourceWaveStream _chimeIntro;
        private RawSourceWaveStream _finalChime;

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

            _chime = await LoadChime($"{_chimeFolderPath}chime.wav");
            _chimeIntro = await LoadChime($"{_chimeFolderPath}chime-intro.wav");
            _finalChime = await LoadChime($"{_chimeFolderPath}final-chime.wav");

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
                Log.Error($"Failed to find chime file at {_chimeFolderPath}chime.wav", ex);
            }

            return chime;
        }

        private void Chime(object sender, ElapsedEventArgs eventArgs)
        {
            try
            {
                Timer timer = (Timer)sender;
                timer.Interval = GetMillisecondsToNextChime(_millisecondsFactor);
                Log.Information($"Next chime will be at {DateTime.Now.AddMilliseconds(timer.Interval)}");

                PlayChime(_chime, _chimeIntro, _finalChime);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                throw;
            }
        }

        private void PlayChime(RawSourceWaveStream chime, RawSourceWaveStream chimeIntro, RawSourceWaveStream finalChime)
        {
            if (null == chime
                || null == chimeIntro
                || null == finalChime)
            {
                Console.Beep();
            }
            else
            {
                int hour = DateTime.Now.Hour;
                if (hour == 0)
                    hour = 12;
                if (hour > 12)
                    hour = hour - 12;

                List<ISampleProvider> providers = new List<ISampleProvider>();
                providers.Add(chimeIntro.ToSampleProvider());

                for (int i = 0; i < hour - 1; i++)
                {
                    MemoryStream stream = new MemoryStream();
                    chime.CopyTo(stream);
                    RawSourceWaveStream waveStream = new RawSourceWaveStream(stream, chime.WaveFormat);
                    waveStream.Position = 0;
                    chime.Position = 0;
                    providers.Add(waveStream.ToSampleProvider());
                }

                providers.Add(finalChime.ToSampleProvider());
                ConcatenatingSampleProvider provida = new ConcatenatingSampleProvider(providers);

                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(provida);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        Task.Delay(1000);
                    }
                }
                chime.Position = 0;
                chimeIntro.Position = 0;
                finalChime.Position = 0;
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
