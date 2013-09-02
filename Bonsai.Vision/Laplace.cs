using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Laplace : Selector<IplImage, IplImage>
    {
        public Laplace()
        {
            ApertureSize = 3;
        }

        public int ApertureSize { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.Channels);
            CV.Laplace(input, output, ApertureSize);
            return output;
        }
    }
}
