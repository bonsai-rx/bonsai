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
        protected override IplImage CreateArray(IplImage source, Depth depth)
        {
            var imageDepth = depth == Depth.F32 ? IplDepth.F32 : source.Depth;
            return new IplImage(source.Size, imageDepth, source.Channels);
        }
    }
}
