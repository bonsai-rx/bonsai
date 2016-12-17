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
        const float DefaultHeight = 480;
        const int DefaultThickness = 2;
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            if (image != null)
            {
                var color = image.Channels == 1 ? Scalar.Real(255) : Scalar.Rgb(255, 0, 0);
                var thickness = DefaultThickness * (int)Math.Ceiling(image.Height / DefaultHeight);
                var circles = value as IEnumerable<Circle>;
                if (circles != null)
                {
                    foreach (var circle in circles)
                    {
                        CV.Circle(image, new Point(circle.Center), (int)circle.Radius, color, thickness);
                    }
                }
                else
                {
                    var circle = (Circle)value;
                    CV.Circle(image, new Point(circle.Center), (int)circle.Radius, color, thickness);                    
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
