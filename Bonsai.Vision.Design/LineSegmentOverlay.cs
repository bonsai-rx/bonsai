using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using System;

[assembly: TypeVisualizer(typeof(LineSegmentOverlay), Target = typeof(MashupSource<ImageMashupVisualizer, LineSegmentVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays a collection of line segments
    /// over an existing image visualizer.
    /// </summary>
    public class LineSegmentOverlay : DialogTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            LineSegmentVisualizer.Draw(visualizer.VisualizerImage, value);
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
