using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Resizes the input image.")]
    public class Resize : Projection<IplImage, IplImage>
    {
        public Resize()
        {
            Interpolation = SubPixelInterpolation.Linear;
        }

        [Description("The size of the output image.")]
        public CvSize Size { get; set; }

        [Description("The interpolation method used to transform individual image elements.")]
        public SubPixelInterpolation Interpolation { get; set; }

        public override IplImage Process(IplImage input)
        {
            if (input.Size != Size)
            {
                var output = new IplImage(Size, input.Depth, input.NumChannels);
                ImgProc.cvResize(input, output, Interpolation);
                return output;
            }
            else return input;
        }
    }
}
