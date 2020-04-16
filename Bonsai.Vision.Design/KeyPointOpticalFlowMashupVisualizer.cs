using System;
using Bonsai;
using Bonsai.Vision.Design;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(KeyPointOpticalFlowMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, KeyPointOpticalFlowVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class KeyPointOpticalFlowMashupVisualizer : MashupTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        public override void Show(object value)
        {
            var tracking = (KeyPointOpticalFlow)value;
            KeyPointOpticalFlowVisualizer.Draw(visualizer.VisualizerImage, tracking);
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
