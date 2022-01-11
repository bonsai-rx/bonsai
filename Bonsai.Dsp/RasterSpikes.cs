using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that rasterizes a sequence of spike waveforms into
    /// sample buffers where spike timepoints are the only non-zero values.
    /// </summary>
    [Description("Rasterizes a sequence of spike waveforms into sample buffers where spike timepoints are the only non-zero values.")]
    public class RasterSpikes : Transform<SpikeWaveformCollection, Mat>
    {
        /// <summary>
        /// Rasterizes an observable sequence of spike waveforms into sample buffers
        /// where spike timepoints are the only non-zero values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="SpikeWaveformCollection"/> objects representing
        /// all detected spike events in each continuous signal buffer.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing continuous sample
        /// buffers in which only indices where spikes were detected will have
        /// non-zero values.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<SpikeWaveformCollection> source)
        {
            return Observable.Defer(() =>
            {
                var offset = 0L;
                byte[] raster = null;
                return source.Select(input =>
                {
                    var size = input.BufferSize;
                    if (raster == null)
                    {
                        raster = new byte[size.Width * size.Height];
                    }

                    var output = new Mat(size, Depth.U8, 1);
                    using (var rasterHeader = Mat.CreateMatHeader(raster, size.Height, size.Width, Depth.U8, 1))
                    {
                        rasterHeader.SetZero();
                        foreach (var spike in input)
                        {
                            var sampleIndex = spike.SampleIndex - offset;
                            if (spike.Waveform != null) sampleIndex += spike.Waveform.Cols;
                            raster[spike.ChannelIndex * size.Width + sampleIndex] = 1;
                        }

                        CV.Copy(rasterHeader, output);
                    }
                    offset += size.Width;
                    return output;
                });
            });
        }
    }
}
