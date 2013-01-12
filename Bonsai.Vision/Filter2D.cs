using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using Bonsai.Dsp;

namespace Bonsai.Vision
{
    public class Filter2D : Filter2D<IplImage>
    {
        protected override IplImage CreateOutput(IplImage input)
        {
            return new IplImage(input.Size, input.Depth, input.NumChannels);
        }
    }
}
