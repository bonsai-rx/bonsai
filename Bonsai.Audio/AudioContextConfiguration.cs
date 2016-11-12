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

        [Description("The playback frequency (Hz) used by the output device.")]
        public int Frequency { get; set; }

        [Description("The refresh frequency (Hz) used by the output device.")]
        public int Refresh { get; set; }
    }
}
