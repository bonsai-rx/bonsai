using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Description("Rasterizes spike waveforms into an output array where spike timepoints are the only non-zero values.")]
    public class RasterSpikes : Transform<SpikeWaveformCollection, Mat>
    {
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
