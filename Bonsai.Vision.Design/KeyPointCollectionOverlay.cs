using System;
using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(KeyPointCollectionOverlay), Target = typeof(MashupSource<ImageMashupVisualizer, KeyPointCollectionVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays a collection of key points
    /// over an existing image visualizer.
    /// </summary>
    public class KeyPointCollectionOverlay : DialogTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var keyPoints = (KeyPointCollection)value;
            KeyPointCollectionVisualizer.Draw(visualizer.VisualizerImage, keyPoints);
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
