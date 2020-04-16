using System;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;

[assembly: TypeVisualizer(typeof(ConnectedComponentCollectionMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, ConnectedComponentCollectionVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class ConnectedComponentCollectionMashupVisualizer : MashupTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            var connectedComponents = (ConnectedComponentCollection)value;
            if (image != null)
            {
                foreach (var component in connectedComponents)
                {
                    DrawingHelper.DrawConnectedComponent(image, component);
                }
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
