using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using OpenTK;

namespace Bonsai.Vision.Design
{
    static class DrawingHelper
    {
        public static void DrawConnectedComponent(IplImage image, ConnectedComponent component)
        {
            DrawConnectedComponent(image, component, Point2f.Zero);
        }

        public static void DrawConnectedComponent(IplImage image, ConnectedComponent component, Point2f offset)
        {
            if (component.Area <= 0) return;
            var centroid = component.Centroid + offset;
            var orientation = component.Orientation;
            var minorAxisOrientation = orientation + Math.PI / 2.0;
            var halfMajorAxis = component.MajorAxisLength * 0.5;
            var halfMinorAxis = component.MinorAxisLength * 0.5;
            var major1 = new Point((int)(centroid.X + halfMajorAxis * Math.Cos(orientation)), (int)(centroid.Y + halfMajorAxis * Math.Sin(orientation)));
            var major2 = new Point((int)(centroid.X - halfMajorAxis * Math.Cos(orientation)), (int)(centroid.Y - halfMajorAxis * Math.Sin(orientation)));
            var minor1 = new Point((int)(centroid.X + halfMinorAxis * Math.Cos(minorAxisOrientation)), (int)(centroid.Y + halfMinorAxis * Math.Sin(minorAxisOrientation)));
            var minor2 = new Point((int)(centroid.X - halfMinorAxis * Math.Cos(minorAxisOrientation)), (int)(centroid.Y - halfMinorAxis * Math.Sin(minorAxisOrientation)));

            CV.DrawContours(image, component.Contour, Scalar.All(255), Scalar.All(0), 0, -1, LineFlags.Connected8, new Point(offset));
            CV.DrawContours(image, component.Contour, Scalar.Rgb(255, 0, 0), Scalar.Rgb(0, 0, 255), 0, 1, LineFlags.Connected8, new Point(offset));
            CV.Line(image, major1, major2, Scalar.Rgb(0, 0, 255));
            CV.Line(image, minor1, minor2, Scalar.Rgb(255, 0, 0));
            CV.Circle(image, new Point(centroid), 2, Scalar.Rgb(255, 0, 0), -1);
        }

        public static Vector2 NormalizePoint(Point point, Size imageSize)
        {
            return new Vector2(
                (point.X * 2f / imageSize.Width) - 1,
                -((point.Y * 2f / imageSize.Height) - 1));
        }
    }
}
