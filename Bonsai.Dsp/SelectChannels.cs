using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Dsp
{
    [Description("Selects a subset of the input channels or reorganizes channel layout.")]
    public class SelectChannels : Transform<Mat, Mat>
    {
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Editor("Bonsai.Dsp.Design.SelectChannelEditor, Bonsai.Dsp.Design", typeof(UITypeEditor))]
        [Description("The channels to include in the output buffer. Reordering and duplicating channels is allowed.")]
        public int[] Channels { get; set; }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                var start = 0;
                var stop = 0;
                var step = -1;
                int[] selectedChannels = null;
                return source.Select(input =>
                {
                    var channels = Channels;
                    if (selectedChannels != channels)
                    {
                        if (channels == null || channels.Length == 0) step = -1;
                        else
                        {
                            start = channels[0];
                            stop = channels[channels.Length - 1] + 1;
                            step = (stop - start) / channels.Length;
                            for (int i = 1; i < channels.Length; i++)
                            {
                                var diff = channels[i] - channels[i - 1];
                                if (diff < 0 || diff != step) step = 0;
                            }
                        }

                        selectedChannels = channels;
                    }

                    if (step < 0) return input;
                    if (start >= 0 && stop <= input.Rows && step > 0) return input.GetRows(start, stop, step);

                    var output = new Mat(selectedChannels.Length, input.Cols, input.Depth, input.Channels);
                    for (int i = 0; i < selectedChannels.Length; i++)
                    {
                        using (var srcRect = input.GetSubRect(new Rect(0, selectedChannels[i], input.Cols, 1)))
                        using (var dstRect = output.GetSubRect(new Rect(0, i, input.Cols, 1)))
                        {
                            CV.Copy(srcRect, dstRect);
                        }
                    }
                    return output;
                });
            });
        }
    }
}
