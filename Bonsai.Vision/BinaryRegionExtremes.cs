using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    [TypeVisualizer("Bonsai.Vision.Design.BinaryRegionExtremesVisualizer, Bonsai.Vision.Design")]
    public class BinaryRegionExtremes : Transform<ConnectedComponent, Tuple<CvPoint2D32f, CvPoint2D32f>>
    {
        public FindExtremesMethod Method { get; set; }

        static double Norm(CvPoint2D32f vector)
        {
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }

        static double Dot(CvPoint2D32f a, CvPoint2D32f b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        static CvPoint2D32f? FindExtremity(int[] points, Func<CvPoint2D32f, double> metric)
        {
            var maxDistance = 0.0;
            CvPoint2D32f? extremePoint = null;
            for (int i = 0; i < points.Length; i += 2)
            {
                var point = new CvPoint2D32f(points[i], points[i + 1]);
                var distance = metric(point);
                if (distance > maxDistance || extremePoint == null)
                {
                    maxDistance = distance;
                    extremePoint = point;
                }
            }

            return extremePoint;
        }

        public override Tuple<CvPoint2D32f, CvPoint2D32f> Process(ConnectedComponent input)
        {
            int[] points;
            var contour = input.Contour;
            var centroid = input.Centroid;
            var orientation = input.Orientation;
            if (contour != null)
            {
                points = new int[contour.Total * 2];
                contour.CopyTo(points);
            }
            else points = new int[0];

            CvPoint2D32f? extremePoint1 = null;
            CvPoint2D32f? extremePoint2 = null;
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
                    var directionVector1 = new CvPoint2D32f((float)Math.Cos(orientation), (float)Math.Sin(orientation));
                    var directionVector2 = new CvPoint2D32f((float)Math.Cos(orientation + Math.PI), (float)Math.Sin(orientation + Math.PI));
                    var normVector1 = Norm(directionVector1);
                    var normVector2 = Norm(directionVector2);
                    extremePoint1 = FindExtremity(points, point => Dot(point, directionVector1) / normVector1);
                    extremePoint2 = FindExtremity(points, point => Dot(point, directionVector2) / normVector2);
                    break;
                case FindExtremesMethod.Radial:
                    extremePoint1 = FindExtremity(points, point => Norm(point - centroid));
                    extremePoint2 = FindExtremity(points, point => Norm(point - extremePoint1.Value));
                    break;
                default:
                    break;
            }

            return Tuple.Create(extremePoint1 ?? new CvPoint2D32f(-1, -1), extremePoint2 ?? new CvPoint2D32f(-1, -1));
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
