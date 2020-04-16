using System;
using Bonsai.Design;
using OpenCV.Net;
using Bonsai;
using Bonsai.Vision.Design;
using Point = OpenCV.Net.Point;

[assembly: TypeVisualizer(typeof(BinaryRegionExtremesMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, BinaryRegionExtremesVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class BinaryRegionExtremesMashupVisualizer : MashupTypeVisualizer
    {
        ImageMashupVisualizer visualizer;

        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            var extremes = (Tuple<Point2f, Point2f>)value;
            CV.Circle(image, new Point(extremes.Item1), 3, Scalar.Rgb(255, 0, 0), -1);
            CV.Circle(image, new Point(extremes.Item2), 3, Scalar.Rgb(0, 255, 0), -1);
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
