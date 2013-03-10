using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Laplace : Transform<IplImage, IplImage>
    {
        public Laplace()
        {
            ApertureSize = 3;
        }

        public int ApertureSize { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            ImgProc.cvLaplace(input, output, ApertureSize);
            return output;
        }
    }
}
