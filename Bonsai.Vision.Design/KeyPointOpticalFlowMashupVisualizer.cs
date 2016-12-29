using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.Vision;
using OpenCV.Net;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(KeyPointOpticalFlowMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, KeyPointOpticalFlowVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class KeyPointOpticalFlowMashupVisualizer : MashupTypeVisualizer
    {
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            var tracking = (KeyPointOpticalFlow)value;
            KeyPointOpticalFlowVisualizer.Draw(visualizer.VisualizerImage, tracking);
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
