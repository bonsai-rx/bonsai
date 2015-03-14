using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
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
        List<List<Point>> polylines;
        IplImageVisualizer visualizer;
        IDisposable subscription;

        public override void Show(object value)
        {
            Point point;
            if (value is Point)
            {
                point = (Point)value;
            }
            else if (value is Point2f)
            {
                point = new Point((Point2f)value);
            }
            else
            {
                var point2d = (Point2d)value;
                point = new Point((int)point2d.X, (int)point2d.Y);
            }

            var image = visualizer.VisualizerImage;
            if (point.X < 0 || point.Y < 0 ||
                point.X >= image.Width || point.Y >= image.Height)
            {
                points = null;
            }
            else if (tracking)
            {
                if (points == null)
                {
                    points = new List<Point>(1);
                    polylines.Add(points);
                }

                points.Add(point);
            }

            if (polylines.Count > 0)
            {
                CV.PolyLine(image, polylines.Select(ps => ps.ToArray()).ToArray(), false, Scalar.Rgb(255, 0, 0), 2);
            }

            CV.Circle(image, point, 3, Scalar.Rgb(0, 255, 0), 3);
            if (!tracking)
            {
                polylines.Clear();
                points = null;
            }
        }

        public override void Load(IServiceProvider provider)
        {
            points = new List<Point>(1);
            polylines = new List<List<Point>>(1);
            visualizer = (IplImageVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
            MouseEventHandler mouseHandler = (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    tracking = !tracking;
                }
            };

            visualizer.VisualizerCanvas.Canvas.MouseClick += mouseHandler;
            subscription = Disposable.Create(() => visualizer.VisualizerCanvas.Canvas.MouseClick -= mouseHandler);
        }

        public override void Unload()
        {
            if (subscription != null)
            {
                subscription.Dispose();
                subscription = null;
                visualizer = null;
                polylines = null;
                points = null;
            }
        }
    }
}
