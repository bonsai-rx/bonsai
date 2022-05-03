using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that creates a 2D point with integer coordinates.
    /// </summary>
    [Description("Creates a 2D point with integer coordinates.")]
    public class CreatePoint : Source<Point>
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the point.
        /// </summary>
        [Description("The x-coordinate of the point.")]
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the point.
        /// </summary>
        [Description("The y-coordinate of the point.")]
        public int Y { get; set; }

        /// <summary>
        /// Generates an observable sequence that contains a single 2D point
        /// with integer coordinates.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Point"/>.
        /// </returns>
        public override IObservable<Point> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Point(X, Y)));
        }

        /// <summary>
        /// Generates an observable sequence of 2D points using the specified
        /// integer coordinates, and where each <see cref="Point"/> object
        /// is emitted only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new 2D points.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Point"/> objects.
        /// </returns>
        public IObservable<Point> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Point(X, Y));
        }
    }
}
