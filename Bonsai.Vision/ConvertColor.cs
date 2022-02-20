using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Converts an image from one color space to another.")]
    public class ConvertColor : Transform<IplImage, IplImage>
    {
        [Description("The color conversion to apply to individual image elements.")]
        public ColorConversion Conversion { get; set; } = ColorConversion.Bgr2Hsv;

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var conversion = Conversion;
                var numChannels = conversion.GetConversionNumChannels();
                return source.Select(input =>
                {
                    if (conversion != Conversion)
                    {
                        conversion = Conversion;
                        numChannels = conversion.GetConversionNumChannels();
                    }

                    var output = new IplImage(input.Size, input.Depth, numChannels);
                    CV.CvtColor(input, output, conversion);
                    return output;
                });
            });
        }
    }
}
