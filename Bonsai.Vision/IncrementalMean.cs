using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Obsolete]
    [Description("Incrementally computes the mean of the incoming image sequence.")]
    public class IncrementalMean : Transform<IplImage, IplImage>
    {
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var count = 0;
                IplImage mean = null;
                return source.Select(input =>
                {
                    if (mean == null)
                    {
                        mean = new IplImage(input.Size, input.Depth, input.Channels);
                        mean.SetZero();
                    }

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.Sub(input, mean, output);
                    CV.ConvertScale(output, output, 1f / ++count, 0);
                    CV.Add(mean, output, mean);
                    CV.Copy(mean, output);
                    return output;
                });
            });
        }
    }
}
