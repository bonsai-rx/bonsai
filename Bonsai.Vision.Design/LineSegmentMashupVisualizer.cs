using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using System;

[assembly: TypeVisualizer(typeof(LineSegmentMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, LineSegmentVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class LineSegmentMashupVisualizer : MashupTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        public override void Show(object value)
        {
            LineSegmentVisualizer.Draw(visualizer.VisualizerImage, value);
        }

        public override void Load(IServiceProvider provider)
        {
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
        }

        public override void Unload()
        {
        }
    }
}
