using System;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents the configuration of 
    /// </summary>
    [Obsolete]
    public class AudioContextConfiguration
    {
        /// <summary>
        /// Gets the name of the output device used for playback.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Gets the sample rate, in Hz, used by the output device.
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Gets the refresh frequency, in Hz, used by the output device.
        /// </summary>
        public int Refresh { get; set; }
    }
}
