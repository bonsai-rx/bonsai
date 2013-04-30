using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using Bonsai.Dsp;

namespace Bonsai.Vision
{
    public class WindowStatistics : WindowStatistics<IplImage>
    {
        protected override IplImage CreateArray(IplImage source, CvMatDepth depth)
        {
            var imageDepth = depth == CvMatDepth.CV_32F ? 32 : source.Depth;
            return new IplImage(source.Size, imageDepth, source.NumChannels);
        }
    }
}
