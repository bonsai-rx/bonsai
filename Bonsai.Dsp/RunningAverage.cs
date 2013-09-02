using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Dsp
{
    public abstract class RunningAverage<TArray> : Transform<TArray, TArray> where TArray : Arr
    {
        [Range(0, 1)]
        [Precision(2, .01)]
        [Editor(DesignTypes.TrackbarEditor, typeof(UITypeEditor))]
        public double Alpha { get; set; }

        protected abstract TArray CreateArray(TArray source, Depth depth);

        public override IObservable<TArray> Process(IObservable<TArray> source)
        {
            return Observable.Defer(() =>
            {
                TArray accumulator = null;
                return source.Select(input =>
                {
                    if (accumulator == null)
                    {
                        accumulator = CreateArray(input, Depth.F32);
                        CV.ConvertScale(input, accumulator, 1, 0);
                        return input;
                    }
                    else
                    {
                        var output = CreateArray(input, 0);
                        CV.RunningAvg(input, accumulator, Alpha);
                        CV.ConvertScale(accumulator, output, 1, 0);
                        return output;
                    }
                });
            });
        }
    }

    public class RunningAverage : RunningAverage<Mat>
    {
        protected override Mat CreateArray(Mat source, Depth depth)
        {
            depth = depth > 0 ? depth : source.Depth;
            return new Mat(source.Rows, source.Cols, depth, source.Channels);
        }
    }
}
