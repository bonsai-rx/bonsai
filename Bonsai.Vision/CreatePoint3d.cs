using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that creates a 3D point with double-precision
    /// floating-point coordinates.
    /// </summary>
    [Description("Creates a 3D point with double-precision floating-point coordinates.")]
    public class CreatePoint3d : Source<Point3d>
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
        /// Gets or sets the z-coordinate of the point.
        /// </summary>
        [Description("The z-coordinate of the point.")]
        public double Z { get; set; }

        /// <summary>
        /// Generates an observable sequence that contains a single 3D point
        /// with double-precision floating-point coordinates.
        /// </summary>
        /// <returns>
        /// A sequence containing the created <see cref="Point3d"/>.
        /// </returns>
        public override IObservable<Point3d> Generate()
        {
            return Observable.Defer(() => Observable.Return(new Point3d(X, Y, Z)));
        }

        /// <summary>
        /// Generates an observable sequence of 3D points using the specified
        /// double-precision floating-point coordinates, and where each
        /// <see cref="Point3d"/> object is emitted only when an observable
        /// sequence raises a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new 3D points.
        /// </param>
        /// <returns>
        /// The sequence of created <see cref="Point3d"/> objects.
        /// </returns>
        public IObservable<Point3d> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => new Point3d(X, Y, Z));
        }
    }
}
