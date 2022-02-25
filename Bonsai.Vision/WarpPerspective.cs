using System;
using System.Linq;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies a perspective transformation to each
    /// image in the sequence.
    /// </summary>
    [DefaultProperty(nameof(Source))]
    [Description("Applies a perspective transformation to each image in the sequence.")]
    public class WarpPerspective : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the coordinates of the four source quadrangle vertices
        /// in the input image.
        /// </summary>
        [Description("The coordinates of the four source quadrangle vertices in the input image.")]
        [Editor("Bonsai.Vision.Design.IplImageInputQuadrangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Point2f[] Source { get; set; }

        /// <summary>
        /// Gets or sets the coordinates of the four corresponding quadrangle
        /// vertices in the output image.
        /// </summary>
        [Description("The coordinates of the four corresponding quadrangle vertices in the output image.")]
        [Editor("Bonsai.Vision.Design.IplImageOutputQuadrangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Point2f[] Destination { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the interpolation and operation flags
        /// for the image warp.
        /// </summary>
        [Description("Specifies the interpolation and operation flags for the image warp.")]
        public WarpFlags Flags { get; set; } = WarpFlags.Linear;

        /// <summary>
        /// Gets or sets the value to which all outlier pixels will be set to.
        /// </summary>
        [Description("The value to which all outlier pixels will be set to.")]
        public Scalar FillValue { get; set; }

        static Point2f[] InitializeQuadrangle(IplImage image)
        {
            return new[]
            {
                new Point2f(0, 0),
                new Point2f(0, image.Height),
                new Point2f(image.Width, image.Height),
                new Point2f(image.Width, 0)
            };
        }

        /// <summary>
        /// Applies a perspective transformation to each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to warp.
        /// </param>
        /// <returns>
        /// The sequence of warped images.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                Point2f[] currentSource = null;
                Point2f[] currentDestination = null;
                var mapMatrix = new Mat(3, 3, Depth.F32, 1);
                return source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    Source = Source ?? InitializeQuadrangle(output);
                    Destination = Destination ?? InitializeQuadrangle(output);

                    if (Source != currentSource || Destination != currentDestination)
                    {
                        currentSource = Source;
                        currentDestination = Destination;
                        CV.GetPerspectiveTransform(currentSource, currentDestination, mapMatrix);
                    }

                    CV.WarpPerspective(input, output, mapMatrix, Flags | WarpFlags.FillOutliers, FillValue);
                    return output;
                });
            });
        }
    }
}
