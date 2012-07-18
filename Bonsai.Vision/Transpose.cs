using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class Transpose : Projection<IplImage, IplImage>
    {
        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(new CvSize(input.Height, input.Width), input.Depth, input.NumChannels);
            Core.cvTranspose(input, output);
            return output;
        }
    }
}
