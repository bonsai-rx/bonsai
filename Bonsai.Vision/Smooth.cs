using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies a smoothing operator to each image
    /// in the sequence. 
    /// </summary>
    [Description("Applies a smoothing operator to each image in the sequence.")]
    public class Smooth : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets a value specifying the smoothing method to be applied.
        /// </summary>
        [Description("Specifies the smoothing method to be applied.")]
        public SmoothMethod SmoothType { get; set; } = SmoothMethod.Gaussian;

        /// <summary>
        /// Gets or sets the aperture width of the smoothing kernel.
        /// </summary>
        [Precision(0, 2)]
        [Range(1, int.MaxValue)]
        [TypeConverter(typeof(SmoothKernelSizeConverter))]
        [Description("The aperture width of the smoothing kernel.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public int Size1 { get; set; } = 3;

        /// <summary>
        /// Gets or sets the aperture height of the smoothing kernel.
        /// </summary>
        [Precision(0, 2)]
        [Range(1, int.MaxValue)]
        [TypeConverter(typeof(SmoothKernelSizeConverter))]
        [Description("The aperture height of the smoothing kernel.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public int Size2 { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation for the first dimension in the case of
        /// Gaussian smoothing.
        /// </summary>
        [Precision(2, 0.1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The standard deviation for the first dimension in the case of Gaussian smoothing.")]
        public double Sigma1 { get; set; }

        /// <summary>
        /// Gets or sets the standard deviation for the second dimension in the case of
        /// Gaussian smoothing.
        /// </summary>
        [Precision(2, 0.1)]
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The standard deviation for the second dimension in the case of Gaussian smoothing.")]
        public double Sigma2 { get; set; }

        /// <summary>
        /// Applies a smoothing operator to each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to smooth.
        /// </param>
        /// <returns>
        /// The sequence of smoothed images.
        /// </returns>
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
                            throw new ArgumentOutOfRangeException(nameof(value), "The size of the filter kernel must be an odd number.");
                        }
                    }
                }

                return kernelSize;
            }
        }
    }
}
