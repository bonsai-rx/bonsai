using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    public class RangeThreshold : Projection<IplImage, IplImage>
    {
        public RangeThreshold()
        {
            Upper = new CvScalar(255, 255, 255, 255);
        }

        [TypeConverter("Bonsai.Vision.Design.RangeScalarConverter, Bonsai.Vision.Design")]
        public CvScalar Lower { get; set; }

        [TypeConverter("Bonsai.Vision.Design.RangeScalarConverter, Bonsai.Vision.Design")]
        public CvScalar Upper { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, 8, 1);
            Core.cvInRangeS(input, Lower, Upper, output);
            return output;
        }
    }
}
