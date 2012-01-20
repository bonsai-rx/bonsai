using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Resize : Projection<IplImage, IplImage>
    {
        public CvSize Size { get; set; }

        public SubPixelInterpolation Interpolation { get; set; }

        public override IplImage Process(IplImage input)
        {
            if (input.Size != Size)
            {
                var output = new IplImage(Size, input.Depth, input.NumChannels);
                ImgProc.cvResize(input, output, Interpolation);
                return output;
            }
            else return input;
        }
    }
}
