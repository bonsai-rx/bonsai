using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that computes the extremities, or endpoints, of each
    /// connected component in the sequence.
    /// </summary>
    [Description("Computes the extremities, or endpoints, of each connected component in the sequence.")]
    [TypeVisualizer("Bonsai.Vision.Design.BinaryRegionExtremesVisualizer, Bonsai.Vision.Design")]
    public class BinaryRegionExtremes : Transform<ConnectedComponent, Tuple<Point2f, Point2f>>
    {
        /// <summary>
        /// Gets or sets a value specifying the method used to compute the extremities
        /// of each connected component.
        /// </summary>
        [Description("Specifies the method used to compute the extremities of each connected component.")]
        public FindExtremesMethod Method { get; set; }

        static double Norm(Point2f vector)
        {
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }

        static double Dot(Point2f a, Point2f b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        static Point2f? FindExtremity(int[] points, Func<Point2f, double> metric)
        {
            var maxDistance = 0.0;
            Point2f? extremePoint = null;
            for (int i = 0; i < points.Length; i += 2)
            {
                var point = new Point2f(points[i], points[i + 1]);
                var distance = metric(point);
                if (distance > maxDistance || extremePoint == null)
                {
                    maxDistance = distance;
                    extremePoint = point;
                }
            }

            return extremePoint;
        }

        /// <summary>
        /// Computes the extremities, or endpoints, of each connected component in
        /// an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of connected components for which to compute the extremities.
        /// </param>
        /// <returns>
        /// A pair of vertices specifying the two extremities, or endpoints, of each
        /// connected component in the sequence. If the connected component has no
        /// vertices, the endpoint coordinates will be set to <see cref="float.NaN"/>.
        /// </returns>
        public override IObservable<Tuple<Point2f, Point2f>> Process(IObservable<ConnectedComponent> source)
        {
            return source.Select(input =>
            {
                int[] points;
                var contour = input.Contour;
                var centroid = input.Centroid;
                var orientation = input.Orientation;
                if (contour != null)
                {
                    points = new int[contour.Count * 2];
                    contour.CopyTo(points);
                }
                else points = new int[0];

                Point2f? extremePoint1 = null;
                Point2f? extremePoint2 = null;
                switch (Method)
                {
                    case FindExtremesMethod.Horizontal:
                        extremePoint1 = FindExtremity(points, point => point.X);
                        extremePoint2 = FindExtremity(points, point => -point.X);
                        break;
                    case FindExtremesMethod.Vertical:
                        extremePoint1 = FindExtremity(points, point => point.Y);
                        extremePoint2 = FindExtremity(points, point => -point.Y);
                        break;
                    case FindExtremesMethod.MajorAxis:
                        var directionX = (float)Math.Cos(orientation);
                        var directionY = (float)Math.Sin(orientation);
                        var directionVector1 = new Point2f(directionX, directionY);
                        var directionVector2 = new Point2f(-directionX, -directionY);
                        extremePoint1 = FindExtremity(points, point => Dot(point, directionVector1));
                        extremePoint2 = FindExtremity(points, point => Dot(point, directionVector2));
                        break;
                    case FindExtremesMethod.MajorAxisVertex:
                        var cos = Math.Cos(orientation);
                        var sin = Math.Sin(orientation);
                        var length = input.MajorAxisLength / 2;
                        extremePoint1 = new Point2f((float)(length * cos + centroid.X), (float)(length * sin + centroid.Y));
                        extremePoint2 = new Point2f((float)(-length * cos + centroid.X), (float)(-length * sin + centroid.Y));
                        break;
                    case FindExtremesMethod.Radial:
                        extremePoint1 = FindExtremity(points, point => Norm(point - centroid));
                        extremePoint2 = FindExtremity(points, point => Norm(point - extremePoint1.Value));
                        break;
                    default:
                        break;
                }

                return Tuple.Create(
                    extremePoint1 ?? new Point2f(float.NaN, float.NaN),
                    extremePoint2 ?? new Point2f(float.NaN, float.NaN));
            });
        }
    }

    /// <summary>
    /// Specifies the method used to compute extremities of connected components.
    /// </summary>
    public enum FindExtremesMethod
    {
        /// <summary>
        /// The first extremity will be the vertex furthest to the right, and the
        /// second extremity the vertex furthest to the left, in image coordinates. 
        /// </summary>
        Horizontal,

        /// <summary>
        /// The first extremity will be the vertex nearest to the bottom of the image,
        /// and the second extremity the vertex nearest to the top of the image. 
        /// </summary>
        Vertical,

        /// <summary>
        /// The first extremity will be the vertex furthest along the major axis of the
        /// ellipse fit to the connected component, moving clockwise, and the second
        /// extremity will be the vertex furthest along the major axis of the ellipse,
        /// moving anti-clockwise.
        /// </summary>
        MajorAxis,

        /// <summary>
        /// The first extremity will be the first clockwise vertex of the ellipse fit
        /// to the connected component and the second extremity the first anti-clockwise
        /// vertex of the ellipse.
        /// </summary>
        MajorAxisVertex,

        /// <summary>
        /// The first extremity will be the vertex furthest away from the centroid of
        /// the connected component, and the second extremity will be the vertex
        /// furthest away from the first extremity.
        /// </summary>
        Radial
    }
}
