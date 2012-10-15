using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Converts the input image into the specified bit depth, with optional linear transformation.")]
    public class ConvertScale : Transform<IplImage, IplImage>
    {
        public ConvertScale()
        {
            Depth = 8;
            Scale = 1;
        }

        [Description("The target bit depth of individual image elements.")]
        public int Depth { get; set; }

        [Description("The optional scale factor to apply to individual image elements.")]
        public double Scale { get; set; }

        [Description("The optional value to be added to individual image elements.")]
        public double Shift { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, Depth, input.NumChannels);
            Core.cvConvertScale(input, output, Scale, Shift);
            return output;
        }
    }
}
