using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class DistanceTransform : Selector<IplImage, IplImage>
    {
        public DistanceTransform()
        {
            DistanceType = OpenCV.Net.DistanceType.L2;
        }

        public DistanceType DistanceType { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, IplDepth.F32, 1);
            CV.DistTransform(input, output, DistanceType);
            return output;
        }
    }
}
