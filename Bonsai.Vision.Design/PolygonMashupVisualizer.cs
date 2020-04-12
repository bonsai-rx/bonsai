using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(PolygonMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, PolygonVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class PolygonMashupVisualizer : MashupTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        public override void Show(object value)
        {
            PolygonVisualizer.Draw(visualizer.VisualizerImage, value);
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
