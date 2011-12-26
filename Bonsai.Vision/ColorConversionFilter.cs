using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ColorConversionFilter : Filter<IplImage, IplImage>
    {
        IplImage output;
        bool conversionChanged;
        ColorConversion conversion;

        public ColorConversionFilter()
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
            if (output == null || conversionChanged)
            {
                var depth = Conversion.GetConversionDepth();
                var numChannels = Conversion.GetConversionNumChannels();
                output = new IplImage(input.Size, depth, numChannels);
                conversionChanged = false;
            }

            ImgProc.cvCvtColor(input, output, conversion);
            return output;
        }

        public override void Unload(WorkflowContext context)
        {
            if (output != null)
            {
                output.Close();
                output = null;
            }
            base.Unload(context);
        }
    }
}
