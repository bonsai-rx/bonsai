using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that calculates the Laplace transform of each image
    /// in the sequence.
    /// </summary>
    [Description("Calculates the Laplace transform of each image in the sequence.")]
    public class Laplace : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the size of the extended Sobel kernel used to compute
        /// derivatives.
        /// </summary>
        [Range(1, 7)]
        [Precision(0, 2)]
        [TypeConverter(typeof(OddKernelSizeConverter))]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The size of the extended Sobel kernel used to compute derivatives.")]
        public int ApertureSize { get; set; } = 3;

        /// <summary>
        /// Calculates the Laplace transform of each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to compute the transform.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects containing the Laplace transform
        /// of each image in the <paramref name="source"/> sequence.
        /// </returns>
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
