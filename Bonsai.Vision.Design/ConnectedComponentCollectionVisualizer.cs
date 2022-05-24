using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Vision;
using System;

[assembly: TypeVisualizer(typeof(ConnectedComponentCollectionVisualizer), Target = typeof(ConnectedComponentCollection))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that displays the properties of a collection
    /// of connected components.
    /// </summary>
    public class ConnectedComponentCollectionVisualizer : IplImageVisualizer
    {
        /// <inheritdoc/>
        public override void Show(object value)
        {
            var offset = Point2f.Zero;
            var connectedComponents = (ConnectedComponentCollection)value;
            var imageSize = connectedComponents.ImageSize;
            if (imageSize == Size.Zero)
            {
                var hasBounds = false;
                var boundsMin = Point.Zero;
                var boundsMax = Point.Zero;
                foreach (var component in connectedComponents)
                {
                    var rect = DrawingHelper.GetBoundingBox(component);
                    if (!hasBounds)
                    {
                        boundsMin = new Point(rect.X, rect.Y);
                        boundsMax = new Point(rect.X + rect.Width, rect.Y + rect.Height);
                        hasBounds = true;
                        continue;
                    }

                    boundsMin.X = Math.Min(boundsMin.X, rect.X);
                    boundsMin.Y = Math.Min(boundsMin.Y, rect.Y);
                    boundsMax.X = Math.Max(boundsMax.X, rect.X + rect.Width);
                    boundsMax.Y = Math.Max(boundsMax.Y, rect.Y + rect.Height);
                }

                imageSize = new Size(boundsMax.X - boundsMin.X, boundsMax.Y - boundsMin.Y);
                offset = new Point2f(-boundsMin);
            }

            var output = new IplImage(imageSize, IplDepth.U8, 3);
            output.SetZero();

            foreach (var component in connectedComponents)
            {
                DrawingHelper.DrawConnectedComponent(output, component, offset);
            }

            base.Show(output);
        }
    }
}
