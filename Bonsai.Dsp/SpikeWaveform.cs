using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    public class SpikeWaveform
    {
        public int ChannelIndex { get; set; }

        public int SampleIndex { get; set; }

        public Mat Waveform { get; set; }
    }
}
