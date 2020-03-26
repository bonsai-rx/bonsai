using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Computes the Canny algorithm for edge detection.")]
    public class Canny : Transform<IplImage, IplImage>
    {
        public Canny()
        {
            ApertureSize = 3;
        }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The first threshold. The smallest threshold is used for edge linking and the largest to find initial edge segments.")]
        public double Threshold1 { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The second threshold. The smallest threshold is used for edge linking and the largest to find initial edge segments.")]
        public double Threshold2 { get; set; }

        [Range(3, 7)]
        [Precision(0, 2)]
        [TypeConverter(typeof(OddKernelSizeConverter))]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("Aperture parameter for the Sobel operator.")]
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
