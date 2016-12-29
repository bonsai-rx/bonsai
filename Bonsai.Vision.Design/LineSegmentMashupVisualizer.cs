using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: TypeVisualizer(typeof(LineSegmentMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, LineSegmentVisualizer>))]
[assembly: TypeVisualizer(typeof(LineSegmentMashupVisualizer), Target = typeof(VisualizerMashup<ContoursVisualizer, LineSegmentVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class LineSegmentMashupVisualizer : MashupTypeVisualizer
    {
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            LineSegmentVisualizer.Draw(visualizer.VisualizerImage, value);
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
