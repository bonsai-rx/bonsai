﻿using Bonsai;
using Bonsai.Vision;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Point = OpenCV.Net.Point;
using Size = OpenCV.Net.Size;

[assembly: TypeVisualizer(typeof(ContourConvexityVisualizer), Target = typeof(ContourConvexity))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that displays the results of convexity analysis
    /// of a polygonal contour.
    /// </summary>
    public class ContourConvexityVisualizer : IplImageVisualizer
    {
        /// <inheritdoc/>
        public override void Show(object value)
        {
            var contourConvexity = (ContourConvexity)value;
            var contour = contourConvexity.Contour;
            var boundingBox = contour != null ? contour.Rect : new Rect(0, 0, 1, 1);
            var output = new IplImage(new Size(boundingBox.Width, boundingBox.Height), IplDepth.U8, 3);
            output.SetZero();

            if (contour != null)
            {
                var offset = new Point2f(-boundingBox.X, -boundingBox.Y);
                CV.DrawContours(output, contour, Scalar.Rgb(0, 255, 0), Scalar.All(0), 0, -1, LineFlags.Connected8, new Point(offset));
                CV.DrawContours(output, contourConvexity.ConvexHull, Scalar.Rgb(0, 255, 0), Scalar.All(0), 0, 1, LineFlags.Connected8, new Point(offset));
                DrawingHelper.DrawConvexityDefects(output, contourConvexity.ConvexityDefects, Scalar.Rgb(204, 0, 204), 1, new Point(offset));
            }
            base.Show(output);
        }
    }
}
