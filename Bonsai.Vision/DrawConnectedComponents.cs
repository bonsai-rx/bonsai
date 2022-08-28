using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that renders each set of connected components
    /// in the sequence as an image.
    /// </summary>
    [Description("Renders each set of connected components in the sequence as an image.")]
    public class DrawConnectedComponents : Transform<ConnectedComponentCollection, IplImage>
    {
        /// <summary>
        /// Renders each set of connected components in an observable sequence
        /// as a new image.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="ConnectedComponentCollection"/> objects to draw.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the result of
        /// rendering each set of connected components as an image.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.ImageSize, IplDepth.U8, 1);
                output.SetZero();

                foreach (var component in input)
                {
                    CV.DrawContours(output, component.Contour, Scalar.All(255), Scalar.All(0), 0, -1);
                }

                return output;
            });
        }
    }
}
