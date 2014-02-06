using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    public class ReferenceChannels : Transform<Mat, Mat>
    {
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Editor("Bonsai.Dsp.Design.SelectChannelEditor, Bonsai.Dsp.Design", typeof(UITypeEditor))]
        public int[] Channels { get; set; }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var channels = Channels;
                if (channels == null || channels.Length == 0) return input;

                var output = new Mat(input.Size, input.Depth, input.Channels);
                var referenceSum = new Mat(1, input.Cols, input.Depth, input.Channels);
                referenceSum.SetZero();
                for (int i = 0; i < channels.Length; i++)
                {
                    using (var referenceChannel = input.GetRow(channels[i]))
                    {
                        CV.Add(referenceSum, referenceChannel, referenceSum);
                    }
                }

                if (channels.Length > 1)
                {
                    CV.ConvertScale(referenceSum, referenceSum, 1f / channels.Length);
                }

                CV.Repeat(referenceSum, output);
                CV.Sub(input, output, output);
                return output;
            });
        }
    }
}
