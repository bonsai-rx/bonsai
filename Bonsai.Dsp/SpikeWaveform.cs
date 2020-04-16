using OpenCV.Net;
using System.Xml.Serialization;

namespace Bonsai.Dsp
{
    public class SpikeWaveform
    {
        public int ChannelIndex { get; set; }

        public long SampleIndex { get; set; }

        [XmlIgnore]
        public Mat Waveform { get; set; }
    }
}
