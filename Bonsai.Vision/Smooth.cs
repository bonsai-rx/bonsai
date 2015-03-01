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
    [Description("Smooths the input image.")]
    public class Smooth : Transform<IplImage, IplImage>
    {
        public Smooth()
        {
            Size1 = 3;
            SmoothType = SmoothMethod.Gaussian;
        }

        [Description("The smoothing method to be applied.")]
        public SmoothMethod SmoothType { get; set; }

        [Precision(0, 2)]
        [Range(1, int.MaxValue)]
        [Description("The aperture width of the smoothing kernel.")]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int Size1 { get; set; }

        [Precision(0, 2)]
        [Range(1, int.MaxValue)]
        [Description("The aperture height of the smoothing kernel.")]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int Size2 { get; set; }

        [Precision(2, 0.1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        [Description("The standard deviation for the first dimension in case of Gaussian smoothing.")]
        public double Sigma1 { get; set; }

        [Precision(2, 0.1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        [Description("The standard deviation for the second dimension in case of Gaussian smoothing.")]
        public double Sigma2 { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.Smooth(input, output, SmoothType, Size1, Size2, Sigma1, Sigma2);
                return output;
            });
        }
    }
}
