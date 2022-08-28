using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that calculates the first, second, third or mixed
    /// image derivatives from the sequence using an extended Sobel operator.
    /// </summary>
    [Description("Calculates the first, second, third or mixed image derivatives from the sequence using an extended Sobel operator.")]
    public class Sobel : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the order of the horizontal derivative.
        /// </summary>
        [Range(0, 6)]
        [Description("The order of the horizontal derivative.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public int XOrder { get; set; } = 1;

        /// <summary>
        /// Gets or sets the order of the vertical derivative.
        /// </summary>
        [Range(0, 6)]
        [Description("The order of the vertical derivative.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public int YOrder { get; set; } = 1;

        /// <summary>
        /// Gets or sets the size of the extended Sobel kernel.
        /// </summary>
        [Range(1, 7)]
        [Precision(0, 2)]
        [TypeConverter(typeof(OddKernelSizeConverter))]
        [Description("The size of the extended Sobel kernel.")]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        public int ApertureSize { get; set; } = 3;

        /// <summary>
        /// Calculates the first, second, third or mixed image derivatives from
        /// an observable sequence using an extended Sobel operator.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to calculate the image derivatives.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the image
        /// derivatives of the original elements in the <paramref name="source"/>
        /// sequence.
        /// </returns>
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
