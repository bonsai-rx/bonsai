using OpenCV.Net;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents a collection of spike waveforms detected by the
    /// <see cref="DetectSpikes"/> operator.
    /// </summary>
    public class SpikeWaveformCollection : Collection<SpikeWaveform>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpikeWaveformCollection"/> class
        /// with the specified buffer size.
        /// </summary>
        /// <param name="bufferSize">
        /// The size of the original buffer in which the spikes in this collection
        /// were detected.
        /// </param>
        public SpikeWaveformCollection(Size bufferSize)
        {
            BufferSize = bufferSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpikeWaveformCollection"/> class
        /// as a wrapper to the specified list of spikes and buffer size.
        /// </summary>
        /// <param name="spikes">
        /// The list of spikes that is wrapped by the new collection.
        /// </param>
        /// <param name="bufferSize">
        /// The size of the original buffer in which the spikes in the list
        /// were detected.
        /// </param>
        public SpikeWaveformCollection(IList<SpikeWaveform> spikes, Size bufferSize)
            : base(spikes)
        {
            BufferSize = bufferSize;
        }

        /// <summary>
        /// Gets the size of the original buffer in which the spikes in this
        /// collection were detected.
        /// </summary>
        public Size BufferSize { get; private set; }
    }
}
