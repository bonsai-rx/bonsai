using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Provides an abstract base class for operators that apply a polar transform
    /// to each image in the sequence.
    /// </summary>
    [Editor("Bonsai.Vision.Design.PolarTransformEditor, Bonsai.Vision.Design", typeof(ComponentEditor))]
    public abstract class PolarTransform : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the transformation center where the output precision is maximal.
        /// </summary>
        [Description("The transformation center where the output precision is maximal.")]
        public Point2f Center { get; set; }

        /// <summary>
        /// Gets or sets the magnitude scale parameter for the polar transformation.
        /// </summary>
        [Description("The magnitude scale parameter for the polar transformation.")]
        public double Magnitude { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the interpolation and operation flags for
        /// the image warp.
        /// </summary>
        [Description("Specifies the interpolation and operation flags for the image warp.")]
        public WarpFlags Flags { get; set; } = WarpFlags.Linear | WarpFlags.FillOutliers;
    }
}
