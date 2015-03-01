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
    [Description("Skips a specified number of samples in the input sequence.")]
    public class Skip : Combinator<Mat, Mat>
    {
        [Description("The number of samples to skip.")]
        public int Count { get; set; }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                var skipSamples = Count;
                var previous = default(Mat);
                return source.SkipWhile(xs =>
                {
                    var skip = skipSamples > 0;
                    if (skip)
                    {
                        skipSamples -= xs.Cols;
                        previous = xs;
                    }
                    return skip;
                }).Select(input =>
                {
                    var bufferOffset = (input.Cols + skipSamples) % input.Cols;
                    if (bufferOffset > 0)
                    {
                        var buffer = new Mat(input.Size, input.Depth, input.Channels);
                        var previousDataLength = buffer.Cols - bufferOffset;
                        var currentDataLength = buffer.Cols - previousDataLength;
                        using (var previousBuffer = buffer.GetSubRect(new Rect(0, 0, previousDataLength, buffer.Rows)))
                        using (var previousInput = previous.GetSubRect(new Rect(bufferOffset, 0, previousDataLength, buffer.Rows)))
                        using (var currentBuffer = buffer.GetSubRect(new Rect(previousDataLength, 0, currentDataLength, buffer.Rows)))
                        using (var currentInput = input.GetSubRect(new Rect(0, 0, currentDataLength, buffer.Rows)))
                        {
                            CV.Copy(previousInput, previousBuffer);
                            CV.Copy(currentInput, currentBuffer);
                        }

                        previous = input;
                        return buffer;
                    }
                    else return input;
                });
            });
        }
    }
}
