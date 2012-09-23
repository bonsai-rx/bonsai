using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Flips the input image around vertical, horizontal or both axes.")]
    public class Flip : Projection<IplImage, IplImage>
    {
        [Description("Specifies how to flip the image.")]
        public FlipMode Mode { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            Core.cvFlip(input, output, Mode);
            return output;
        }
    }
}
