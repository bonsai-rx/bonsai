using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class Sobel : Selector<IplImage, IplImage>
    {
        public Sobel()
        {
            ApertureSize = 3;
        }

        [Range(0, 6)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int XOrder { get; set; }

        [Range(0, 6)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int YOrder { get; set; }

        [Range(1, 7)]
        [Precision(0, 2)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int ApertureSize { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, IplDepth.F32, input.Channels);
            CV.Sobel(input, output, XOrder, YOrder, ApertureSize);
            return output;
        }
    }
}
