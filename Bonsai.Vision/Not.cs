using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using Bonsai;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the per-element bitwise inversion of the input image.")]
    public class Not : Selector<IplImage, IplImage>
    {
        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.Channels);
            CV.Not(input, output);
            return output;
        }
    }
}
