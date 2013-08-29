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
        [Editor(DesignTypes.TrackbarEditor, typeof(UITypeEditor))]
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
                        accumulator = new IplImage(input.Size, 32, input.NumChannels);
                        temp = new IplImage(accumulator.Size, accumulator.Depth, accumulator.NumChannels);
                    }

                    var output = new IplImage(input.Size, input.Depth, input.NumChannels);
                    Core.cvSub(input, accumulator, temp, CvArr.Null);
                    ImgProc.cvRunningAvg(input, accumulator, Alpha, CvArr.Null);
                    Core.cvConvertScale(temp, output, 1, 0);
                    return output;
                });
            });
        }
    }
}
