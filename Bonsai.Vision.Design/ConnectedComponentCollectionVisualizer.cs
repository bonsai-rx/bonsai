using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Vision;

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
            var connectedComponents = (ConnectedComponentCollection)value;
            var output = new IplImage(connectedComponents.ImageSize, IplDepth.U8, 3);
            output.SetZero();

            foreach (var component in connectedComponents)
            {
                DrawingHelper.DrawConnectedComponent(output, component);
            }

            base.Show(output);
        }
    }
}
