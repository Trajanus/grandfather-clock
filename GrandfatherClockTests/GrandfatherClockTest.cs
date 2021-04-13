using FishmanIndustries;
using NAudio.Wave;
using System;
using Xunit;

namespace GrandfatherClockTest
{
    public class GrandfatherClockTest
    {
        [Fact]
        public void GrandfatherClockPlayChime_NullParameters_ThrowsException()
        {
            GrandfatherClockOptions options = new GrandfatherClockOptions
            {
                Volume = 0.4f
            };

            GrandfatherClock clock = new GrandfatherClock(options);
            Assert.Throws<ArgumentNullException>(() => clock.PlayChime(null, null, null));
        }
    }
}
