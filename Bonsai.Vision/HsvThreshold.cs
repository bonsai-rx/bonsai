using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    public class HsvThreshold : Transform<IplImage, IplImage>
    {
        public HsvThreshold()
        {
            Upper = new CvScalar(179, 255, 255, 255);
        }

        [TypeConverter("Bonsai.Vision.Design.HsvScalarConverter, Bonsai.Vision.Design")]
        public CvScalar Lower { get; set; }

        [TypeConverter("Bonsai.Vision.Design.HsvScalarConverter, Bonsai.Vision.Design")]
        public CvScalar Upper { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, 8, 1);
            Core.cvInRangeS(input, Lower, Upper, output);
            return output;
        }
    }
}
