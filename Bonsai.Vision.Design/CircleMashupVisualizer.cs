using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(CircleMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, CircleVisualizer>))]
[assembly: TypeVisualizer(typeof(CircleMashupVisualizer), Target = typeof(VisualizerMashup<ContoursVisualizer, CircleVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class CircleMashupVisualizer : MashupTypeVisualizer
    {
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            CircleVisualizer.Draw(visualizer.VisualizerImage, value);
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
