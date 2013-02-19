using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class Mask : Transform<IplImage, IplImage, IplImage>
    {
        public override IplImage Process(IplImage first, IplImage second)
        {
            var output = new IplImage(first.Size, first.Depth, first.NumChannels);
            output.SetZero();
            Core.cvCopy(first, output, second ?? CvArr.Null);
            return output;
        }
    }
}
