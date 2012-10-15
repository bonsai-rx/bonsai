using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class Canny : Transform<IplImage, IplImage>
    {
        public Canny()
        {
            ApertureSize = 3;
        }

        [Range(0, 255)]
        [Editor(DesignTypes.TrackbarEditor, typeof(UITypeEditor))]
        public double Threshold1 { get; set; }

        [Range(0, 255)]
        [Editor(DesignTypes.TrackbarEditor, typeof(UITypeEditor))]
        public double Threshold2 { get; set; }

        [Range(3, 7)]
        [Precision(0, 2)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int ApertureSize { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, 8, 1);
            ImgProc.cvCanny(input, output, Threshold1, Threshold2, ApertureSize);
            return output;
        }
    }
}
