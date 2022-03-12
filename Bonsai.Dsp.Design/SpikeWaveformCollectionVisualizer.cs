using Bonsai;
using Bonsai.Dsp;
using Bonsai.Dsp.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(SpikeWaveformCollectionVisualizer), Target = typeof(SpikeWaveformCollection))]

namespace Bonsai.Dsp.Design
{
    /// <summary>
    /// Provides a type visualizer that displays a collection of spike waveforms and
    /// overlays a specified number of past spike waveforms for each independent channel.
    /// </summary>
    public class SpikeWaveformCollectionVisualizer : SpikeWaveformCollectionVisualizer<WaveformView>
    {
    }

    /// <summary>
    /// Provides a base class to display a collection of spike waveforms.
    /// </summary>
    /// <typeparam name="TWaveformView">
    /// A type derived from <see cref="WaveformView"/> which will control how data is displayed.
    /// </typeparam>
    public class SpikeWaveformCollectionVisualizer<TWaveformView> : MatVisualizer<TWaveformView> where TWaveformView : WaveformView, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpikeWaveformCollectionVisualizer"/> class.
        /// </summary>
        public SpikeWaveformCollectionVisualizer()
        {
            OverlayChannels = false;
            WaveformBufferLength = 10;
        }

        /// <inheritdoc/>
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
