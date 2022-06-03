using System;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;

[assembly: TypeVisualizer(typeof(ConnectedComponentCollectionOverlay), Target = typeof(MashupSource<ImageMashupVisualizer, ConnectedComponentCollectionVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays the collection of connected
    /// components over an existing image visualizer.
    /// </summary>
    public class ConnectedComponentCollectionOverlay : DialogTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            var connectedComponents = (ConnectedComponentCollection)value;
            if (image != null)
            {
                foreach (var component in connectedComponents)
                {
                    DrawingHelper.DrawConnectedComponent(image, component);
                }
            }
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(MashupVisualizer));
        }

        /// <inheritdoc/>
        public override void Unload()
        {
        }
    }
}
