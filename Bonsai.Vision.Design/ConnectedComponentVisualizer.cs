using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Vision;

[assembly: TypeVisualizer(typeof(ConnectedComponentVisualizer), Target = typeof(ConnectedComponent))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that displays the properties of a cluster of
    /// connected pixels.
    /// </summary>
    public class ConnectedComponentVisualizer : IplImageVisualizer
    {
        /// <inheritdoc/>
        public override void Show(object value)
        {
            var connectedComponent = (ConnectedComponent)value;
            var boundingBox = DrawingHelper.GetBoundingBox(connectedComponent);
            var output = new IplImage(new Size(boundingBox.Width, boundingBox.Height), IplDepth.U8, 3);
            output.SetZero();

            DrawingHelper.DrawConnectedComponent(output, connectedComponent, new Point2f(-boundingBox.X, -boundingBox.Y));
            base.Show(output);
        }
    }
}
