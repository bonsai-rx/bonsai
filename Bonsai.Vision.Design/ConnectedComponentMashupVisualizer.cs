using System;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;

[assembly: TypeVisualizer(typeof(ConnectedComponentMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, ConnectedComponentVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class ConnectedComponentMashupVisualizer : MashupTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            var connectedComponent = (ConnectedComponent)value;
            if (image != null)
            {
                DrawingHelper.DrawConnectedComponent(image, connectedComponent);
            }
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
