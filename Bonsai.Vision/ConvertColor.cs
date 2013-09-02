using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Converts an image from one color space to another.")]
    public class ConvertColor : Selector<IplImage, IplImage>
    {
        IplDepth depth;
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

        public override IplImage Process(IplImage input)
        {
            if (conversionChanged)
            {
                depth = Conversion.GetConversionDepth();
                numChannels = Conversion.GetConversionNumChannels();
                conversionChanged = false;
            }

            var output = new IplImage(input.Size, depth, numChannels);
            CV.CvtColor(input, output, conversion);
            return output;
        }
    }
}
