using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Design;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;

[assembly: TypeVisualizer(typeof(ContourConvexityMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, ContourConvexityVisualizer>))]
[assembly: TypeVisualizer(typeof(ContourConvexityMashupVisualizer), Target = typeof(VisualizerMashup<ContoursVisualizer, ContourConvexityVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class ContourConvexityMashupVisualizer : MashupTypeVisualizer
    {
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            var contourConvexity = (ContourConvexity)value;
            var contour = contourConvexity.Contour;

            if (image != null && contour != null)
            {
                CV.DrawContours(image, contourConvexity.ConvexHull, Scalar.Rgb(0, 255, 0), Scalar.All(0), 0);
                DrawingHelper.DrawConvexityDefects(image, contourConvexity.ConvexityDefects, Scalar.Rgb(204, 0, 204));
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
