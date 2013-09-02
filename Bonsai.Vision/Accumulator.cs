using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class Accumulator : Transform<IplImage, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                IplImage sum = null;
                return source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    sum = IplImageHelper.EnsureImageFormat(sum, input.Size, IplDepth.F32, input.Channels);
                    CV.Acc(input, sum);
                    if (sum.Depth == input.Depth) CV.Copy(sum, output);
                    else CV.Convert(sum, output);
                    return output;
                });
            });
        }
    }
}
