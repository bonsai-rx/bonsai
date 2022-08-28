using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that projects each element of the sequence into a
    /// buffered array based on element count information.
    /// </summary>
    [Obsolete]
    [Description("Projects each element of the sequence into a buffered array based on element count information.")]
    public class ElementCountBuffer : Combinator<Mat, Mat>
    {
        /// <summary>
        /// Gets or sets the number of elements in each buffer.
        /// </summary>
        [Description("The number of elements in each buffer.")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the optional number of elements to skip between the creation of each buffer.
        /// </summary>
        [Description("The optional number of elements to skip between the creation of each buffer.")]
        public int? Skip { get; set; }

        /// <summary>
        /// Projects each element of the sequence into a buffered array based on
        /// element count information.
        /// </summary>
        /// <param name="source">
        /// A sequence of multi-channel matrices to buffer.
        /// </param>
        /// <returns>
        /// The sequence of arrays containing buffers of samples of the
        /// multi-channel matrix sequence.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Create<Mat>(observer =>
            {
                var skipCount = 0;
                var count = Count;
                var skip = Skip.GetValueOrDefault(count);
                var activeBuffers = new List<SampleBuffer>();
                return source.Subscribe(input =>
                {
                    try
                    {
                        // Update pending windows
                        activeBuffers.RemoveAll(buffer =>
                        {
                            buffer.Update(input, 0);
                            if (buffer.Completed)
                            {
                                // Window is ready, emit
                                observer.OnNext(buffer.Samples);
                                return true;
                            }

                            return false;
                        });

                        var index = 0;
                        while ((index + skipCount) < input.Cols)
                        {
                            // Create new window and reset skip counter
                            index += skipCount;
                            skipCount = skip;
                            var buffer = new SampleBuffer(input, count, index);
                            buffer.Update(input, index);
                            if (buffer.Completed)
                            {
                                // Window is ready, emit
                                observer.OnNext(buffer.Samples);
                            }
                            // Window is missing data, add to list
                            else activeBuffers.Add(buffer);
                        }

                        // Remove remainder of input samples from skip counter
                        skipCount -= input.Cols - index;
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                error => observer.OnError(error),
                () => observer.OnCompleted());
            });
        }
    }
}
