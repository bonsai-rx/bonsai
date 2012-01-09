using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ConvertColor : Projection<IplImage, IplImage>
    {
        int depth;
        int numChannels;
        bool conversionChanged;
        ColorConversion conversion;

        public ConvertColor()
        {
            Conversion = ColorConversion.BGR2GRAY;
        }

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
            ImgProc.cvCvtColor(input, output, conversion);
            return output;
        }
    }
}
