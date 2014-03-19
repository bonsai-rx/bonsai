using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    public class ElementCountBuffer : Combinator<Mat, Mat>
    {
        public int Count { get; set; }

        public int? Skip { get; set; }

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
                        var buffer = new SampleBuffer(input, count);
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
                },
                error => observer.OnError(error),
                () => observer.OnCompleted());
            });
        }
    }
}
