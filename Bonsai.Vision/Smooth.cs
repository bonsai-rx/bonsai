using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Smooth : Selector<IplImage, IplImage>
    {
        public SmoothMethod SmoothType { get; set; }

        public int Size1 { get; set; }

        public int Size2 { get; set; }

        public double Sigma1 { get; set; }

        public double Sigma2 { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.Channels);
            CV.Smooth(input, output, SmoothType, Size1, Size2, Sigma1, Sigma2);
            return output;
        }
    }
}
