using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class IncrementalMean : Transform<IplImage, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Create<IplImage>(observer =>
            {
                var count = 0;
                IplImage mean = null;

                var process = source.Select(input =>
                {
                    if (mean == null)
                    {
                        mean = new IplImage(input.Size, input.Depth, input.NumChannels);
                        mean.SetZero();
                    }

                    var output = new IplImage(input.Size, input.Depth, input.NumChannels);
                    Core.cvSub(input, mean, output, CvArr.Null);
                    Core.cvConvertScale(output, output, 1f / count++, 0);
                    Core.cvAdd(mean, output, mean, CvArr.Null);
                    Core.cvCopy(mean, output, CvArr.Null);
                    return output;
                }).Subscribe(observer);

                var close = Disposable.Create(() =>
                {
                    if (mean != null)
                    {
                        mean.Close();
                    }
                });

                return new CompositeDisposable(process, close);
            });
        }
    }
}
