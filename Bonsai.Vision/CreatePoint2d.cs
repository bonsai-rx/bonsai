using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that creates a 2D point with double-precision
    /// floating-point coordinates.
    /// </summary>
    [Description("Creates a 2D point with double-precision floating-point coordinates.")]
    public class CreatePoint2d : Source<Point2d>
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the point.
        /// </summary>
        [Description("The x-coordinate of the point.")]
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the point.
        /// </summary>
        [Description("The y-coordinate of the point.")]
        public double Y { get; set; }

        /// <summary>
        /// Generates an observable sequence that contains a single 2D point
        /// with double-precision floating-point coordinates.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Point2d"/>.
        /// </returns>
        public override IObservable<Point2d> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Point2d(X, Y)));
        }

        /// <summary>
        /// Generates an observable sequence of 2D points using the specified
        /// double-precision floating-point coordinates, and where each
        /// <see cref="Point2d"/> object is emitted only when an observable
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new 2D points.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Point2d"/> objects.
        /// </returns>
        public IObservable<Point2d> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Point2d(X, Y));
        }
    }
}
