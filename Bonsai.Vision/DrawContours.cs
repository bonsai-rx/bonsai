using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that renders each set of contours in the sequence
    /// as an image.
    /// </summary>
    [Description("Renders each set of contours in the sequence as an image.")]
    public class DrawContours : Transform<Contours, IplImage>
    {
        /// <summary>
        /// Gets or sets the maximum level of the contour hierarchy to draw.
        /// </summary>
        [Description("The maximum level of the contour hierarchy to draw.")]
        public int MaxLevel { get; set; } = 1;

        /// <summary>
        /// Gets or sets the thickness of the lines with which the contours are drawn.
        /// If negative, the contour interiors are drawn.
        /// </summary>
        /// <remarks></remarks>
        [Description("The thickness of the lines with which the contours are drawn. If negative, the contour interiors are drawn.")]
        public int Thickness { get; set; } = -1;

        /// <summary>
        /// Renders each set of contours in an observable sequence as a new image.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Contours"/> objects to draw.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the result of
        /// rendering each set of contours as an image.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<Contours> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.ImageSize, IplDepth.U8, 1);
                output.SetZero();

                if (input.FirstContour != null)
                {
                    CV.DrawContours(output, input.FirstContour, Scalar.All(255), Scalar.All(0), MaxLevel, Thickness);
                }

                return output;
            });
        }

        /// <summary>
        /// Renders each contour in an observable sequence as a new image.
        /// </summary>
        /// <param name="source">The sequence of contour objects to draw.</param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the result of
        /// rendering each contour as an image.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Contour> source)
        {
            return source.Select(input =>
            {
                var rect = input.Rect;
                var output = new IplImage(new Size(rect.Width, rect.Height), IplDepth.U8, 1);
                output.SetZero();
                CV.DrawContours(output, input, Scalar.All(255), Scalar.All(0), MaxLevel, Thickness, LineFlags.Connected8, new Point(-rect.X, -rect.Y));
                return output;
            });
        }
    }
}
