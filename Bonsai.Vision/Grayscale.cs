using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Grayscale : Projection<IplImage, IplImage>
    {
        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, 8, 1);
            ImgProc.cvCvtColor(input, output, ColorConversion.BGR2GRAY);
            return output;
        }
    }
}
