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
            return Observable.Create<IplImage>(observer =>
            {
                IplImage sum = null;

                var process = source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.NumChannels);
                    sum = IplImageHelper.EnsureImageFormat(sum, input.Size, 32, input.NumChannels);
                    ImgProc.cvAcc(input, sum, CvArr.Null);
                    if (sum.Depth == input.Depth) Core.cvCopy(sum, output);
                    else Core.cvConvert(sum, output);
                    return output;
                }).Subscribe(observer);

                var close = Disposable.Create(() =>
                {
                    if (sum != null)
                    {
                        sum.Dispose();
                    }
                });

                return new CompositeDisposable(process, close);
            });
        }
    }
}
