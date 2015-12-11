using Bonsai;
using Bonsai.Dsp;
using Bonsai.Dsp.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(SpikeWaveformCollectionVisualizer), Target = typeof(SpikeWaveformCollection))]

namespace Bonsai.Dsp.Design
{
    public class SpikeWaveformCollectionVisualizer : SpikeWaveformCollectionVisualizer<WaveformView>
    {
    }

    public class SpikeWaveformCollectionVisualizer<TWaveformView> : MatVisualizer<TWaveformView> where TWaveformView : WaveformView, new()
    {
        public SpikeWaveformCollectionVisualizer()
        {
            OverlayChannels = false;
            WaveformBufferLength = 10;
        }

        public override void Show(object value)
        {
            var spikes = (SpikeWaveformCollection)value;
            var bufferSize = spikes.BufferSize;
            double[] samples = null;
            Graph.EnsureWaveform(bufferSize.Height, bufferSize.Width);
            foreach (var spike in spikes)
            {
                if (spike.Waveform == null) continue;
                if (samples == null)
                {
                    samples = new double[spike.Waveform.Cols];
                }

                using (var sampleHeader = Mat.CreateMatHeader(samples))
                {
                    CV.Convert(spike.Waveform, sampleHeader);
                }

                Graph.UpdateWaveform(spike.ChannelIndex, samples, bufferSize.Height, samples.Length);
            }

            InvalidateGraph();
        }
    }
}
