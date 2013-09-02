using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the absolute difference between the two input images.")]
    public class AbsoluteDifference : Selector<IplImage, IplImage, IplImage>
    {
        public override IplImage Process(IplImage first, IplImage second)
        {
            var output = new IplImage(first.Size, first.Depth, first.Channels);
            CV.AbsDiff(first, second, output);
            return output;
        }
    }
}
