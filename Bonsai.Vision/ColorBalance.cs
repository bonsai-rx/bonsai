using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class ColorBalance : Transform<IplImage, IplImage>
    {
        public ColorBalance()
        {
            Scale = CvScalar.All(1);
        }

        [Precision(2, .01)]
        [Range(0, int.MaxValue)]
        [TypeConverter("Bonsai.Vision.Design.BgraScalarConverter, Bonsai.Vision.Design")]
        public CvScalar Scale { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Create<IplImage>(observer =>
            {
                IplImage channel1 = null;
                IplImage channel2 = null;
                IplImage channel3 = null;
                IplImage channel4 = null;

                var process = source.Select(input =>
                {
                    channel1 = IplImageHelper.EnsureImageFormat(channel1, input.Size, 8, 1);
                    if (input.NumChannels > 1) channel2 = IplImageHelper.EnsureImageFormat(channel2, input.Size, 8, 1);
                    if (input.NumChannels > 2) channel3 = IplImageHelper.EnsureImageFormat(channel3, input.Size, 8, 1);
                    if (input.NumChannels > 3) channel4 = IplImageHelper.EnsureImageFormat(channel4, input.Size, 8, 1);

                    var output = new IplImage(input.Size, input.Depth, input.NumChannels);
                    Core.cvSplit(input, channel1, channel2 ?? CvArr.Null, channel3 ?? CvArr.Null, channel4 ?? CvArr.Null);

                    if (channel1 != null) Core.cvConvertScale(channel1, channel1, Scale.Val0, 0);
                    if (channel2 != null) Core.cvConvertScale(channel2, channel2, Scale.Val1, 0);
                    if (channel3 != null) Core.cvConvertScale(channel3, channel3, Scale.Val2, 0);
                    if (channel4 != null) Core.cvConvertScale(channel4, channel4, Scale.Val3, 0);
                    Core.cvMerge(channel1, channel2 ?? CvArr.Null, channel3 ?? CvArr.Null, channel4 ?? CvArr.Null, output);
                    return output;
                }).Subscribe(observer);

                var close = Disposable.Create(() =>
                {
                    if (channel1 != null)
                    {
                        channel1.Close();
                    }

                    if (channel2 != null)
                    {
                        channel2.Close();
                    }

                    if (channel3 != null)
                    {
                        channel3.Close();
                    }

                    if (channel4 != null)
                    {
                        channel4.Close();
                    }
                });

                return new CompositeDisposable(process, close);
            });
        }
    }
}
