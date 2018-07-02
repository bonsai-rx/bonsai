using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(PolygonMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, PolygonVisualizer>))]
[assembly: TypeVisualizer(typeof(PolygonMashupVisualizer), Target = typeof(VisualizerMashup<ContoursVisualizer, PolygonVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class PolygonMashupVisualizer : MashupTypeVisualizer
    {
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            PolygonVisualizer.Draw(visualizer.VisualizerImage, value);
        }

        public override void Load(IServiceProvider provider)
        {
            visualizer = (IplImageVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
        }

        public override void Unload()
        {
        }
    }
}
