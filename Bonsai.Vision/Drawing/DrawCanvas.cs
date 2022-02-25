using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that renders the sequence of operations in each canvas
    /// of the sequence to create a new image.
    /// </summary>
    [Description("Renders the sequence of operations in each canvas of the sequence to create a new image.")]
    public class DrawCanvas : Transform<Canvas, IplImage>
    {
        /// <summary>
        /// Renders the sequence of operations in each canvas of an observable sequence
        /// to create a new image.
        /// </summary>
        /// <param name="source">A sequence of canvas objects.</param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the result of
        /// the cumulative application of all the drawing operations.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<Canvas> source)
        {
            return source.Select(canvas => canvas.Draw());
        }
    }
}
