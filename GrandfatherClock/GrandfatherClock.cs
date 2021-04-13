using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FishmanIndustries
{
    public class GrandfatherClock
    {
        private GrandfatherClockOptions _options;
        public GrandfatherClock(GrandfatherClockOptions options)
        {
            _options = options;
        }

        public void PlayChime(RawSourceWaveStream chime, RawSourceWaveStream chimeIntro, RawSourceWaveStream finalChime)
        {
            if (null == chimeIntro)
                throw new ArgumentNullException(nameof(chimeIntro));

            if (null == chime)
                throw new ArgumentNullException(nameof(chime));

            if (null == finalChime)
                throw new ArgumentNullException(nameof(finalChime));

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

            var outputDevice = new WaveOutEvent();
            outputDevice.Volume = _options.Volume;
            outputDevice.Init(provida);
            outputDevice.Play();
            outputDevice.PlaybackStopped += OnPlaybackStopped;

            chime.Position = 0;
            chimeIntro.Position = 0;
            finalChime.Position = 0;
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
