using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using Bonsai;

namespace VideoAnalyzer.Vision
{
    public class Not : Projection<IplImage, IplImage>
    {
        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            Core.cvNot(input, output);
            return output;
        }
    }
}
