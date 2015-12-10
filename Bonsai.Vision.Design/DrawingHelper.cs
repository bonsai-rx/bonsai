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

            if (component.Patch != null)
            {
                var target = image;
                var patch = component.Patch;
                var mask = patch.Channels == 1 ? patch : null;
                try
                {
                    if (component.Contour != null)
                    {
                        var rect = component.Contour.Rect;
                        mask = new IplImage(patch.Size, patch.Depth, 1);
                        mask.SetZero();
                        CV.DrawContours(mask, component.Contour, Scalar.All(255), Scalar.All(0), 0, -1, LineFlags.Connected8, new Point(-rect.X, -rect.Y));
                        if (image.Width != rect.Width || image.Height != rect.Height)
                        {
                            target = image.GetSubRect(component.Contour.Rect);
                        }
                    }

                    if (patch.Channels != target.Channels)
                    {
                        var conversion = patch.Channels > image.Channels
                            ? ColorConversion.Bgr2Gray
                            : ColorConversion.Gray2Bgr;
                        patch = new IplImage(patch.Size, patch.Depth, image.Channels);
                        CV.CvtColor(component.Patch, patch, conversion);
                    }

                    CV.Copy(patch, target, mask);
                }
                finally
                {
                    if (patch != component.Patch) patch.Dispose();
                    if (mask != component.Patch) mask.Dispose();
                    if (target != image) target.Dispose();
                }
            }
            else if (component.Contour != null)
            {
                CV.DrawContours(image, component.Contour, Scalar.All(255), Scalar.All(0), 0, -1, LineFlags.Connected8, new Point(offset));
            }
            
            if (component.Contour != null)
            {
                CV.DrawContours(image, component.Contour, Scalar.Rgb(255, 0, 0), Scalar.Rgb(0, 0, 255), 0, 1, LineFlags.Connected8, new Point(offset));
            }
            CV.Line(image, major1, major2, Scalar.Rgb(0, 0, 255));
            CV.Line(image, minor1, minor2, Scalar.Rgb(255, 0, 0));
            CV.Circle(image, new Point(centroid), 2, Scalar.Rgb(255, 0, 0), -1);
        }

        public static void DrawConvexityDefects(IplImage image, Seq convexityDefects, Scalar color, int thickness = 1)
        {
            DrawConvexityDefects(image, convexityDefects, color, thickness, Point.Zero);
        }

        public static void DrawConvexityDefects(IplImage image, Seq convexityDefects, Scalar color, int thickness, Point offset)
        {
            if (convexityDefects != null && convexityDefects.Count > 0)
            {
                var defects = convexityDefects.ToArray<ConvexityDefect>();
                foreach (var defect in defects)
                {
                    var startPoint = defect.Start;
                    var endPoint = defect.End;
                    var depthPoint = defect.DepthPoint;
                    var surfacePoint = new Point((startPoint.X + endPoint.X) / 2 + offset.X, (startPoint.Y + endPoint.Y) / 2 + offset.Y);
                    var depthOffset = new Point(depthPoint.X + offset.X, depthPoint.Y + offset.Y);
                    CV.Line(image, depthOffset, surfacePoint, color, thickness);
                }
            }
        }

        public static Vector2 NormalizePoint(Point point, Size imageSize)
        {
            return new Vector2(
                (point.X * 2f / imageSize.Width) - 1,
                -((point.Y * 2f / imageSize.Height) - 1));
        }
    }
}
