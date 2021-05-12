using FishmanIndustries;
using NAudio.Wave;
using System;
using System.IO;
using Xunit;

namespace GrandfatherClockTest
{
    public class GrandfatherClockTest
    {
        [Fact]
        public void GrandfatherClockConstructor_NullParameters_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new GrandfatherClock(null, null, null, null));
        }

        [Fact]
        public void GrandfatherClockConstructor_NullOptions_ThrowsException()
        {
            RawSourceWaveStream stream = new RawSourceWaveStream(new byte[0], 0, 0, WaveFormat.CreateALawFormat(0, 0));

            Assert.Throws<ArgumentNullException>(() => new GrandfatherClock(null, stream, stream, stream));
        }

        [Fact]
        public void GrandfatherClockConstructor_NullChimeIntro_ThrowsException()
        {
            GrandfatherClockOptions options = new GrandfatherClockOptions
            {
                Volume = 0.4f
            };

            RawSourceWaveStream stream = new RawSourceWaveStream(new byte[0], 0, 0, WaveFormat.CreateALawFormat(0, 0));

            Assert.Throws<ArgumentNullException>(() => new GrandfatherClock(options, null, stream, stream));
        }

        [Fact]
        public void GrandfatherClockConstructor_NullChime_ThrowsException()
        {
            GrandfatherClockOptions options = new GrandfatherClockOptions
            {
                Volume = 0.4f
            };

            RawSourceWaveStream stream = new RawSourceWaveStream(new byte[0], 0, 0, WaveFormat.CreateALawFormat(0, 0));

            Assert.Throws<ArgumentNullException>(() => new GrandfatherClock(options, stream, null, stream));
        }

        [Fact]
        public void GrandfatherClockConstructor_NullFinalChime_ThrowsException()
        {
            GrandfatherClockOptions options = new GrandfatherClockOptions
            {
                Volume = 0.4f
            };

            RawSourceWaveStream stream = new RawSourceWaveStream(new byte[0], 0, 0, WaveFormat.CreateALawFormat(0, 0));

            Assert.Throws<ArgumentNullException>(() => new GrandfatherClock(options, stream, stream, null));
        }

        [Fact]
        public void GrandfatherClockPlayChime_ValidParameters_ValidObject()
        {
            GrandfatherClockOptions options = new GrandfatherClockOptions
            {
                Volume = 0.4f
            };

            RawSourceWaveStream stream = new RawSourceWaveStream(new byte[0], 0, 0, WaveFormat.CreateALawFormat(0, 0));

            GrandfatherClock clock = new GrandfatherClock(options, stream, stream, stream);

            Assert.True(clock.Options.Volume == options.Volume);
        }
    }
}
