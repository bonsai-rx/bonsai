using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(ConnectedComponentCollectionMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, ConnectedComponentCollectionVisualizer>))]
[assembly: TypeVisualizer(typeof(ConnectedComponentCollectionMashupVisualizer), Target = typeof(VisualizerMashup<ContoursVisualizer, ConnectedComponentCollectionVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class ConnectedComponentCollectionMashupVisualizer : MashupTypeVisualizer
    {
        IplImageVisualizer visualizer;

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
            visualizer = (IplImageVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
        }

        public override void Unload()
        {
        }
    }
}
