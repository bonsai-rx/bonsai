using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using System;

[assembly: TypeVisualizer(typeof(CircleMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, CircleVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class CircleMashupVisualizer : MashupTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        public override void Show(object value)
        {
            CircleVisualizer.Draw(visualizer.VisualizerImage, value);
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
