using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Flip : Projection<IplImage, IplImage>
    {
        public FlipMode Mode { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            Core.cvFlip(input, output, Mode);
            return output;
        }
    }
}
