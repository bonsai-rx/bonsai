using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    public class HsvThresholdFilter : Filter<IplImage, IplImage>
    {
        IplImage output;

        public HsvThresholdFilter()
        {
            Upper = new CvScalar(179, 255, 255, 255);
        }

        [TypeConverter("Bonsai.Vision.Design.HsvScalarConverter, Bonsai.Vision.Design")]
        public CvScalar Lower { get; set; }

        [TypeConverter("Bonsai.Vision.Design.HsvScalarConverter, Bonsai.Vision.Design")]
        public CvScalar Upper { get; set; }

        public override IplImage Process(IplImage input)
        {
            if (output == null || output.Width != input.Width || output.Height != input.Height)
            {
                output = new IplImage(input.Size, 8, 1);
            }

            Core.cvInRangeS(input, Lower, Upper, output);
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
