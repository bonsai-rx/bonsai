using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
