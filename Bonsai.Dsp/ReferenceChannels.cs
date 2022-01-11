using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that subtracts a reference channel from all the
    /// individual rows in a 2D array sequence.
    /// </summary>
    [Description("Subtracts a reference channel from all the individual rows in a 2D array sequence.")]
    public class ReferenceChannels : Transform<Mat, Mat>
    {
        /// <summary>
        /// Gets or sets the indices of the channels to use as reference. If not specified,
        /// the average of all the array channels is used.
        /// </summary>
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Editor("Bonsai.Dsp.Design.SelectChannelEditor, Bonsai.Dsp.Design", DesignTypes.UITypeEditor)]
        [Description("The indices of the channels to use as reference. If not specified, the average of all the array channels is used.")]
        public int[] Channels { get; set; }

        /// <summary>
        /// Subtracts a reference channel from all the individual rows in an observable sequence
        /// of 2D array values.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D array values.
        /// </param>
        /// <returns>
        /// A sequence of 2D array values, where the reference channel for each array
        /// has been subtracted from every row.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var channels = Channels;
                var output = new Mat(input.Size, input.Depth, input.Channels);
                var reference = new Mat(1, input.Cols, input.Depth, input.Channels);
                if (channels == null || channels.Length == 0)
                {
                    if (input.Depth != Depth.F32)
                    {
                        var temp = new Mat(reference.Rows, reference.Cols, Depth.F32, reference.Channels);
                        CV.Reduce(input, temp, 0, ReduceOperation.Avg);
                        CV.Convert(temp, reference);
                    }
                    else CV.Reduce(input, reference, 0, ReduceOperation.Avg);
                }
                else if (channels.Length == 1)
                {
                    CV.Copy(input.GetRow(channels[0]), reference);
                }
                else
                {
                    var sum = input.Depth != Depth.F32
                        ? new Mat(reference.Rows, reference.Cols, Depth.F32, reference.Channels)
                        : reference;
                    sum.SetZero();
                    for (int i = 0; i < channels.Length; i++)
                    {
                        using (var referenceChannel = input.GetRow(channels[i]))
                        {
                            CV.Add(sum, referenceChannel, sum);
                        }
                    }

                    CV.ConvertScale(sum, reference, 1f / channels.Length);
                }

                CV.Repeat(reference, output);
                CV.Sub(input, output, output);
                return output;
            });
        }
    }
}
