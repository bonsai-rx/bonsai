using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class ConvertScale : Projection<IplImage, IplImage>
    {
        public ConvertScale()
        {
            Depth = 8;
            Scale = 1;
        }

        public int Depth { get; set; }

        public double Scale { get; set; }

        public double Shift { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, Depth, input.NumChannels);
            Core.cvConvertScale(input, output, Scale, Shift);
            return output;
        }
    }
}
