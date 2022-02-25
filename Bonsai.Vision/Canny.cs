using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies the Canny algorithm for edge detection to each
    /// image in the sequence.
    /// </summary>
    [Description("Applies the Canny algorithm for edge detection to each image in the sequence.")]
    public class Canny : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the first threshold. The smallest threshold is used for edge
        /// linking and the largest to find initial edge segments.
        /// </summary>
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The first threshold. The smallest threshold is used for edge linking and the largest to find initial edge segments.")]
        public double Threshold1 { get; set; }

        /// <summary>
        /// Gets or sets the second threshold. The smallest threshold is used for edge
        /// linking and the largest to find initial edge segments.
        /// </summary>
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The second threshold. The smallest threshold is used for edge linking and the largest to find initial edge segments.")]
        public double Threshold2 { get; set; }

        /// <summary>
        /// Gets or sets the aperture parameter for the Sobel operator.
        /// </summary>
        [Range(3, 7)]
        [Precision(0, 2)]
        [TypeConverter(typeof(OddKernelSizeConverter))]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The aperture parameter for the Sobel operator.")]
        public int ApertureSize { get; set; } = 3;

        /// <summary>
        /// Applies the Canny algorithm for edge detection to each image in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">The sequence of images from which to extract edges.</param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects where each non-zero pixel
        /// represents an image element which has been classified as an edge.
        /// </returns>
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
