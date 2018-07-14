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
    [Description("Calculates the first, second, third or mixed image derivatives using an extended Sobel operator.")]
    public class Sobel : Transform<IplImage, IplImage>
    {
        public Sobel()
        {
            ApertureSize = 3;
            XOrder = 1;
            YOrder = 1;
        }

        [Range(0, 6)]
        [Description("The order of the horizontal derivative.")]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int XOrder { get; set; }

        [Range(0, 6)]
        [Description("The order of the vertical derivative.")]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int YOrder { get; set; }

        [Range(1, 7)]
        [Precision(0, 2)]
        [TypeConverter(typeof(OddKernelSizeConverter))]
        [Description("The size of the extended Sobel kernel.")]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int ApertureSize { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, IplDepth.F32, input.Channels);
                CV.Sobel(input, output, XOrder, YOrder, ApertureSize);
                return output;
            });
        }
    }
}
