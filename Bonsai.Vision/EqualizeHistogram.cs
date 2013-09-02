using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class EqualizeHistogram : Selector<IplImage, IplImage>
    {
        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.Channels);
            CV.EqualizeHist(input, output);
            return output;
        }
    }
}
