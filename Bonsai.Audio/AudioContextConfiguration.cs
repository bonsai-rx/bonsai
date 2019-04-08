using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    public class AudioContextConfiguration
    {
        [Description("The name of the output device used for playback.")]
        [TypeConverter(typeof(PlaybackDeviceNameConverter))]
        public string DeviceName { get; set; }

        [Description("The sample rate, in Hz, used by the output device.")]
        public int SampleRate { get; set; }

        [Description("The refresh frequency, in Hz, used by the output device.")]
        public int Refresh { get; set; }
    }
}
