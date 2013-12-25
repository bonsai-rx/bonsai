using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class MotionSegmentation : Transform<IplImage, IplImage>
    {
        [Range(0, 1)]
        [Precision(2, .01)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        public double Alpha { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                IplImage temp = null;
                IplImage accumulator = null;
                return source.Select(input =>
                {
                    if (accumulator == null)
                    {
                        accumulator = new IplImage(input.Size, IplDepth.F32, input.Channels);
                        temp = new IplImage(accumulator.Size, accumulator.Depth, accumulator.Channels);
                    }

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.Sub(input, accumulator, temp);
                    CV.RunningAvg(input, accumulator, Alpha);
                    CV.ConvertScale(temp, output, 1, 0);
                    return output;
                });
            });
        }
    }
}
