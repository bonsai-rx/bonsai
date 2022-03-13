using System;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;

[assembly: TypeVisualizer(typeof(ConnectedComponentCollectionMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, ConnectedComponentCollectionVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays the collection of connected
    /// components over an existing image visualizer.
    /// </summary>
    public class ConnectedComponentCollectionMashupVisualizer : MashupTypeVisualizer
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
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
        }

        /// <inheritdoc/>
        public override void Unload()
        {
        }
    }
}
