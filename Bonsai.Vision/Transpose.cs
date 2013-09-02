using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Transpose : Selector<IplImage, IplImage>
    {
        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(new Size(input.Height, input.Width), input.Depth, input.Channels);
            CV.Transpose(input, output);
            return output;
        }
    }
}
