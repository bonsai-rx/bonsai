using System;
using System.Collections.Generic;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that delays the input signal by the specified number of samples.
    /// </summary>
    [Description("Delays the input signal by the specified number of samples.")]
    public class Delay : Transform<Mat, Mat>
    {
        /// <summary>
        /// Gets or sets the number of samples by which to delay the input signal.
        /// </summary>
        /// <remarks>
        /// To avoid changing the number of buffers in the sequence, the beginning of the
        /// signal will be padded with zeros.
        /// </remarks>
        [Description("The number of samples by which to delay the input signal.")]
        public int Count { get; set; }

        /// <summary>
        /// Delays the input signal by the specified number of samples.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the waveform of the
        /// signal to delay.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing the waveform of the
        /// delayed signal.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                var count = Count;
                int bufferIndex = 0;
                Queue<Mat> buffer = null;
                if (count == 0) return source;
                else return source.Select(input =>
                {
                    if (buffer == null)
                    {
                        buffer = new Queue<Mat>();
                        var delayBuffer = new Mat(input.Rows, count, input.Depth, input.Channels);
                        buffer.Enqueue(delayBuffer);
                        delayBuffer.SetZero();
                    }

                    buffer.Enqueue(input);
                    var output = new Mat(input.Rows, input.Cols, input.Depth, input.Channels);
                    var outputRemainder = output.Cols;
                    var outputIndex = 0;

                    while (outputRemainder > 0)
                    {
                        var currentBuffer = buffer.Peek();
                        var sampleCount = Math.Min(currentBuffer.Cols - bufferIndex, outputRemainder);
                        using (var bufferRoi = currentBuffer.GetSubRect(new Rect(bufferIndex, 0, sampleCount, currentBuffer.Rows)))
                        using (var outputRoi = output.GetSubRect(new Rect(outputIndex, 0, sampleCount, currentBuffer.Rows)))
                        {
                            CV.Copy(bufferRoi, outputRoi);
                            outputRemainder -= sampleCount;
                            outputIndex += sampleCount;
                            bufferIndex = (bufferIndex + sampleCount) % currentBuffer.Cols;
                            if (bufferIndex == 0) buffer.Dequeue();
                        }
                    }

                    return output;
                });
            });
        }
    }
}
