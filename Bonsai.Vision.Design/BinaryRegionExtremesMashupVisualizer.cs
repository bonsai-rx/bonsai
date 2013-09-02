using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Design;
using System.Windows.Forms;
using System.Drawing;
using OpenCV.Net;
using Bonsai.Expressions;
using Bonsai.Dag;
using Bonsai;
using Bonsai.Vision.Design;
using Point = OpenCV.Net.Point;

[assembly: TypeVisualizer(typeof(BinaryRegionExtremesMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, BinaryRegionExtremesVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class BinaryRegionExtremesMashupVisualizer : MashupTypeVisualizer
    {
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            var image = visualizer.VisualizerImage;
            var extremes = (Tuple<Point2f, Point2f>)value;
            CV.Circle(image, new Point(extremes.Item1), 3, Scalar.Rgb(255, 0, 0), -1);
            CV.Circle(image, new Point(extremes.Item2), 3, Scalar.Rgb(0, 255, 0), -1);
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
