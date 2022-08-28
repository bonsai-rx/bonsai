using OpenCV.Net;
using System.Xml.Serialization;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents a single spike event extracted from an input signal.
    /// </summary>
    public class SpikeWaveform
    {
        /// <summary>
        /// Gets or sets a value indicating in which channel the spike event
        /// was detected.
        /// </summary>
        public int ChannelIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating in which sample the spike event
        /// was detected.
        /// </summary>
        public long SampleIndex { get; set; }

        /// <summary>
        /// Gets or sets the optional waveform of the spike event.
        /// </summary>
        [XmlIgnore]
        public Mat Waveform { get; set; }
    }
}
