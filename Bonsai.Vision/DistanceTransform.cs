using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that calculates the distance to the closest zero pixel
    /// for all non-zero pixels of each image in the sequence.
    /// </summary>
    [Description("Calculates the distance to the closest zero pixel for all non-zero pixels of each image in the sequence.")]
    public class DistanceTransform : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets a value specifying the type of distance function to use.
        /// </summary>
        [TypeConverter(typeof(DistanceTypeConverter))]
        [Description("Specifies the type of distance function to use.")]
        public DistanceType DistanceType { get; set; } = DistanceType.L2;

        /// <summary>
        /// Calculates the distance to the closest zero pixel for all non-zero pixels
        /// of each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to compute the distance transform.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects where each pixel contains
        /// the calculated distance from the original image element to the closest
        /// zero pixel.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, IplDepth.F32, 1);
                CV.DistTransform(input, output, DistanceType);
                return output;
            });
        }
    }
}
