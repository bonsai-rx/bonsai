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
            var output = new IplImage(input.Size, 32, 1);
            ImgProc.cvDistTransform(input, output, DistanceType, 3, null, CvArr.Null, DistanceLabel.ConnectedComponent);
            return output;
        }
    }
}
