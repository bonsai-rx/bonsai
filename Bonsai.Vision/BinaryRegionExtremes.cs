using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Computes the extremities of individual connected components.")]
    [TypeVisualizer("Bonsai.Vision.Design.BinaryRegionExtremesVisualizer, Bonsai.Vision.Design")]
    public class BinaryRegionExtremes : Transform<ConnectedComponent, Tuple<Point2f, Point2f>>
    {
        [Description("The method used to compute the extremities of each connected component.")]
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
                        var directionVector1 = new Point2f((float)Math.Cos(orientation), (float)Math.Sin(orientation));
                        var directionVector2 = new Point2f((float)Math.Cos(orientation + Math.PI), (float)Math.Sin(orientation + Math.PI));
                        extremePoint1 = FindExtremity(points, point => Dot(point, directionVector1));
                        extremePoint2 = FindExtremity(points, point => Dot(point, directionVector2));
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

    public enum FindExtremesMethod
    {
        Horizontal,
        Vertical,
        MajorAxis,
        Radial
    }
}
