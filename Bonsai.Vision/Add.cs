using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Add : Selector<IplImage, IplImage, IplImage>
    {
        public override IplImage Process(IplImage first, IplImage second)
        {
            var output = new IplImage(first.Size, first.Depth, first.NumChannels);
            Core.cvAdd(first, second, output, CvArr.Null);
            return output;
        }
    }
}
