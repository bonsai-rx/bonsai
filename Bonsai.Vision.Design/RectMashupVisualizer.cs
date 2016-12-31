using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(RectMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, RectVisualizer>))]
[assembly: TypeVisualizer(typeof(RectMashupVisualizer), Target = typeof(VisualizerMashup<ContoursVisualizer, RectVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class RectMashupVisualizer : MashupTypeVisualizer
    {
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            RectVisualizer.Draw(visualizer.VisualizerImage, value);
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
