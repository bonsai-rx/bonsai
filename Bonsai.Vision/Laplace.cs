using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Calculates the laplacian of the input image.")]
    public class Laplace : Transform<IplImage, IplImage>
    {
        public Laplace()
        {
            ApertureSize = 3;
        }

        [Range(1, 7)]
        [Precision(0, 2)]
        [TypeConverter(typeof(OddKernelSizeConverter))]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The size of the extended Sobel kernel used to compute derivatives.")]
        public int ApertureSize { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, input.Channels);
                CV.Laplace(input, output, ApertureSize);
                return output;
            });
        }
    }
}
