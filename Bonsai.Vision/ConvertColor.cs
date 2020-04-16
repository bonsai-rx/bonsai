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
        int numChannels;
        bool conversionChanged;
        ColorConversion conversion;

        public ConvertColor()
        {
            Conversion = ColorConversion.Bgr2Hsv;
        }

        [Description("The color conversion to apply to individual image elements.")]
        public ColorConversion Conversion
        {
            get { return conversion; }
            set
            {
                conversion = value;
                conversionChanged = true;
            }
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                if (conversionChanged)
                {
                    numChannels = Conversion.GetConversionNumChannels();
                    conversionChanged = false;
                }

                var output = new IplImage(input.Size, input.Depth, numChannels);
                CV.CvtColor(input, output, conversion);
                return output;
            });
        }
    }
}
