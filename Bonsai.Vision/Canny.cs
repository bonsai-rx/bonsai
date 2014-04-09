using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public double Threshold1 { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public double Threshold2 { get; set; }

        [Range(3, 7)]
        [Precision(0, 2)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int ApertureSize { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, IplDepth.U8, 1);
                CV.Canny(input, output, Threshold1, Threshold2, ApertureSize);
                return output;
            });
        }
    }
}
