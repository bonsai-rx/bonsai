using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    public class RangeThreshold : Selector<IplImage, IplImage>
    {
        public RangeThreshold()
        {
            Upper = new Scalar(255, 255, 255, 255);
        }

        [TypeConverter("Bonsai.Vision.Design.RangeScalarConverter, Bonsai.Vision.Design")]
        public Scalar Lower { get; set; }

        [TypeConverter("Bonsai.Vision.Design.RangeScalarConverter, Bonsai.Vision.Design")]
        public Scalar Upper { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, IplDepth.U8, 1);
            CV.InRangeS(input, Lower, Upper, output);
            return output;
        }
    }
}
