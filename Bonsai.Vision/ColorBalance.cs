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
            Scale = Scalar.All(1);
        }

        [Precision(2, .01)]
        [Range(0, int.MaxValue)]
        [TypeConverter("Bonsai.Vision.Design.BgraScalarConverter, Bonsai.Vision.Design")]
        public Scalar Scale { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                IplImage channel1 = null;
                IplImage channel2 = null;
                IplImage channel3 = null;
                IplImage channel4 = null;
                return source.Select(input =>
                {
                    channel1 = IplImageHelper.EnsureImageFormat(channel1, input.Size, IplDepth.U8, 1);
                    if (input.Channels > 1) channel2 = IplImageHelper.EnsureImageFormat(channel2, input.Size, IplDepth.U8, 1);
                    if (input.Channels > 2) channel3 = IplImageHelper.EnsureImageFormat(channel3, input.Size, IplDepth.U8, 1);
                    if (input.Channels > 3) channel4 = IplImageHelper.EnsureImageFormat(channel4, input.Size, IplDepth.U8, 1);

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.Split(input, channel1, channel2, channel3, channel4);

                    if (channel1 != null) CV.ConvertScale(channel1, channel1, Scale.Val0, 0);
                    if (channel2 != null) CV.ConvertScale(channel2, channel2, Scale.Val1, 0);
                    if (channel3 != null) CV.ConvertScale(channel3, channel3, Scale.Val2, 0);
                    if (channel4 != null) CV.ConvertScale(channel4, channel4, Scale.Val3, 0);
                    CV.Merge(channel1, channel2, channel3, channel4, output);
                    return output;
                });
            });
        }
    }
}
