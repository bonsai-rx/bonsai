using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(ConnectedComponentMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, ConnectedComponentVisualizer>))]
[assembly: TypeVisualizer(typeof(ConnectedComponentMashupVisualizer), Target = typeof(VisualizerMashup<ContoursVisualizer, ConnectedComponentVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class ConnectedComponentMashupVisualizer : MashupTypeVisualizer
    {
        IplImageVisualizer visualizer;

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
            visualizer = (IplImageVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
        }

        public override void Unload()
        {
        }
    }
}
