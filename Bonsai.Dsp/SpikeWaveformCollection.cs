using OpenCV.Net;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bonsai.Dsp
{
    public class SpikeWaveformCollection : Collection<SpikeWaveform>
    {
        public SpikeWaveformCollection(Size bufferSize)
        {
            BufferSize = bufferSize;
        }

        public SpikeWaveformCollection(IList<SpikeWaveform> spikes, Size bufferSize)
            : base(spikes)
        {
            BufferSize = bufferSize;
        }

        public Size BufferSize { get; private set; }
    }
}
