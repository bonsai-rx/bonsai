using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(PointMashupVisualizer), Target = typeof(VisualizerMashup<IplImageVisualizer, PointVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class PointMashupVisualizer : MashupTypeVisualizer
    {
        bool tracking;
        List<Point> points;
        IplImageVisualizer visualizer;

        public override void Show(object value)
        {
            if (value is Point)
            {
                points.Add((Point)value);
            }
            else if (value is Point2f)
            {
                points.Add(new Point((Point2f)value));
            }
            else
            {
                var point2d = (Point2d)value;
                points.Add(new Point((int)point2d.X, (int)point2d.Y));
            }

            var image = visualizer.VisualizerImage;
            if (points.Count > 1)
            {
                CV.PolyLine(image, new[] { points.Skip(1).ToArray() }, false, Scalar.Rgb(255, 0, 0), 2);
            }

            CV.Circle(image, points[points.Count - 1], 3, Scalar.Rgb(0, 255, 0), 3);
            if (!tracking)
            {
                points.Clear();
            }
        }

        public override void Load(IServiceProvider provider)
        {
            points = new List<Point>(1);
            visualizer = (IplImageVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
            visualizer.VisualizerCanvas.Canvas.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    tracking = !tracking;
                }
            };
        }

        public override void Unload()
        {
        }
    }
}
