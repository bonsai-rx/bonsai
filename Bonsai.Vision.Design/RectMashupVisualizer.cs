using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using System;

[assembly: TypeVisualizer(typeof(RectMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, RectVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class RectMashupVisualizer : MashupTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        public override void Show(object value)
        {
            RectVisualizer.Draw(visualizer.VisualizerImage, value);
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
