using System;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;

[assembly: TypeVisualizer(typeof(ConnectedComponentMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, ConnectedComponentVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays a cluster of connected pixels
    /// over an existing image visualizer.
    /// </summary>
    public class ConnectedComponentMashupVisualizer : MashupTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            var connectedComponent = (ConnectedComponent)value;
            if (image != null)
            {
                DrawingHelper.DrawConnectedComponent(image, connectedComponent);
            }
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
        }

        /// <inheritdoc/>
        public override void Unload()
        {
        }
    }
}
