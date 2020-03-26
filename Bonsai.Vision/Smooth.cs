using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

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
        [TypeConverter(typeof(SmoothKernelSizeConverter))]
        [Description("The aperture width of the smoothing kernel.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public int Size1 { get; set; }

        [Precision(0, 2)]
        [Range(1, int.MaxValue)]
        [TypeConverter(typeof(SmoothKernelSizeConverter))]
        [Description("The aperture height of the smoothing kernel.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public int Size2 { get; set; }

        [Precision(2, 0.1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The standard deviation for the first dimension in case of Gaussian smoothing.")]
        public double Sigma1 { get; set; }

        [Precision(2, 0.1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
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

        class SmoothKernelSizeConverter : Int32Converter
        {
            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                var kernelSize = (int)base.ConvertFrom(context, culture, value);
                var smooth = context.Instance as Smooth;
                if (smooth != null)
                {
                    var smoothType = smooth.SmoothType;
                    if (smoothType == SmoothMethod.Gaussian ||
                        smoothType == SmoothMethod.Median)
                    {
                        if (kernelSize % 2 == 0)
                        {
                            throw new ArgumentOutOfRangeException("value", "The size of the filter kernel must be an odd number.");
                        }
                    }
                }

                return kernelSize;
            }
        }
    }
}
