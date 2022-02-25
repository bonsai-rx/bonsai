using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that extracts the topological skeleton of each
    /// image in the sequence.
    /// </summary>
    [Description("Extracts the topological skeleton of each image in the sequence.")]
    public class Skeletonization : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the function used to compute the distance transform for each pixel.
        /// </summary>
        [TypeConverter(typeof(DistanceTypeConverter))]
        [Description("The function used to compute the distance transform for each pixel.")]
        public DistanceType DistanceType { get; set; } = DistanceType.L2;

        /// <summary>
        /// Gets or sets the size of the extended Sobel kernel used to compute derivatives.
        /// </summary>
        [Range(1, 31)]
        [Precision(0, 2)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The size of the extended Sobel kernel used to compute derivatives.")]
        public int LaplacianAperture { get; set; } = 3;

        /// <summary>
        /// Gets or sets the second-derivative cutoff used to isolate skeleton lines.
        /// </summary>
        [Precision(2, 0.1)]
        [Range(int.MinValue, 0.0)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The second-derivative cutoff used to isolate skeleton lines.")]
        public double RidgeThreshold { get; set; }

        /// <summary>
        /// Extracts the topological skeleton of each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to extract the topological skeleton.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects where each non-zero pixel
        /// belongs to the extracted topological skeleton of the original image.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var distance = default(IplImage);
                var laplacian = default(IplImage);
                return source.Select(input =>
                {
                    var output = new IplImage(input.Size, IplDepth.U8, 1);
                    distance = IplImageHelper.EnsureImageFormat(distance, input.Size, IplDepth.F32, 1);
                    laplacian = IplImageHelper.EnsureImageFormat(laplacian, input.Size, IplDepth.F32, 1);

                    CV.DistTransform(input, distance, DistanceType);
                    CV.Laplace(distance, laplacian, LaplacianAperture);
                    CV.Threshold(laplacian, output, RidgeThreshold, 255, ThresholdTypes.BinaryInv);
                    return output;
                });
            });
        }
    }
}
