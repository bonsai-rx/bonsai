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
            DrawConnectedComponent(image, component, CvPoint2D32f.Zero);
        }

        public static void DrawConnectedComponent(IplImage image, ConnectedComponent component, CvPoint2D32f offset)
        {
            var centroid = component.Centroid + offset;
            var orientation = component.Orientation;
            var minorAxisOrientation = orientation + Math.PI / 2.0;
            var halfMajorAxis = component.MajorAxisLength * 0.5;
            var halfMinorAxis = component.MinorAxisLength * 0.5;
            var major1 = new CvPoint((int)(centroid.X + halfMajorAxis * Math.Cos(orientation)), (int)(centroid.Y + halfMajorAxis * Math.Sin(orientation)));
            var major2 = new CvPoint((int)(centroid.X - halfMajorAxis * Math.Cos(orientation)), (int)(centroid.Y - halfMajorAxis * Math.Sin(orientation)));
            var minor1 = new CvPoint((int)(centroid.X + halfMinorAxis * Math.Cos(minorAxisOrientation)), (int)(centroid.Y + halfMinorAxis * Math.Sin(minorAxisOrientation)));
            var minor2 = new CvPoint((int)(centroid.X - halfMinorAxis * Math.Cos(minorAxisOrientation)), (int)(centroid.Y - halfMinorAxis * Math.Sin(minorAxisOrientation)));

            Core.cvDrawContours(image, component.Contour, CvScalar.All(255), CvScalar.All(0), 0, -1, 8, new CvPoint(offset));
            Core.cvDrawContours(image, component.Contour, CvScalar.Rgb(255, 0, 0), CvScalar.Rgb(0, 0, 255), 0, 1, 8, new CvPoint(offset));
            Core.cvLine(image, major1, major2, CvScalar.Rgb(0, 0, 255), 1, 8, 0);
            Core.cvLine(image, minor1, minor2, CvScalar.Rgb(255, 0, 0), 1, 8, 0);
            Core.cvCircle(image, new CvPoint(centroid), 2, CvScalar.Rgb(255, 0, 0), -1, 8, 0);
        }

        public static Vector2 NormalizePoint(CvPoint point, CvSize imageSize)
        {
            return new Vector2(
                (point.X * 2f / imageSize.Width) - 1,
                -((point.Y * 2f / imageSize.Height) - 1));
        }
    }
}
