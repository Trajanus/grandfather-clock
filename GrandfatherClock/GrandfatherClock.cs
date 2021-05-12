using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FishmanIndustries
{
    public class GrandfatherClock
    {
        private readonly RawSourceWaveStream _chime;
        private readonly RawSourceWaveStream _chimeIntro;
        private readonly RawSourceWaveStream _finalChime;

        public GrandfatherClockOptions Options { get; }

        public GrandfatherClock(GrandfatherClockOptions options, RawSourceWaveStream chimeIntro, RawSourceWaveStream chime, RawSourceWaveStream finalChime)
        {
            if (null == options)
                throw new ArgumentNullException(nameof(options));

            if (null == chimeIntro)
                throw new ArgumentNullException(nameof(chimeIntro));

            if (null == chime)
                throw new ArgumentNullException(nameof(chime));

            if (null == finalChime)
                throw new ArgumentNullException(nameof(finalChime));

            Options = options;
            _chimeIntro = chimeIntro;
            _chime = chime;
            _finalChime = finalChime;
        }

        public void PlayChime()
        {
            int hour = DateTime.Now.Hour;
            if (hour == 0)
                hour = 12;
            if (hour > 12)
                hour = hour - 12;

            List<ISampleProvider> providers = new List<ISampleProvider>();
            providers.Add(_chimeIntro.ToSampleProvider());

            for (int i = 0; i < hour - 1; i++)
            {
                MemoryStream stream = new MemoryStream();
                _chime.CopyTo(stream);
                RawSourceWaveStream waveStream = new RawSourceWaveStream(stream, _chime.WaveFormat);
                waveStream.Position = 0;
                _chime.Position = 0;
                providers.Add(waveStream.ToSampleProvider());
            }

            providers.Add(_finalChime.ToSampleProvider());
            ConcatenatingSampleProvider provida = new ConcatenatingSampleProvider(providers);

            var outputDevice = new WaveOutEvent();
            outputDevice.Volume = Options.Volume;
            outputDevice.Init(provida);
            outputDevice.Play();
            outputDevice.PlaybackStopped += OnPlaybackStopped;

            _chime.Position = 0;
            _chimeIntro.Position = 0;
            _finalChime.Position = 0;
        }

        private void OnPlaybackStopped(object sender, EventArgs e)
        {
            WaveOutEvent outputDevice = sender as WaveOutEvent;
            if (null != outputDevice && outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                outputDevice.Dispose();
            }
        }
    }
}
