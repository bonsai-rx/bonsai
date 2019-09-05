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
        TrackingMode tracking;
        Queue<Point> points;
        Queue<Queue<Point>> polylines;
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
                var point2f = (Point2f)value;
                point.X = float.IsNaN(point2f.X) ? -1 : (int)point2f.X;
                point.Y = float.IsNaN(point2f.Y) ? -1 : (int)point2f.Y;
            }
            else
            {
                var point2d = (Point2d)value;
                point.X = double.IsNaN(point2d.X) ? -1 : (int)point2d.X;
                point.Y = double.IsNaN(point2d.Y) ? -1 : (int)point2d.Y;
            }

            var image = visualizer.VisualizerImage;
            if (point.X < 0 || point.Y < 0 ||
                point.X >= image.Width || point.Y >= image.Height)
            {
                points = null;
            }
            else if (tracking != TrackingMode.None)
            {
                if (points == null)
                {
                    points = new Queue<Point>(1);
                    polylines.Enqueue(points);
                }

                points.Enqueue(point);
                if (tracking == TrackingMode.Fixed)
                {
                    var head = polylines.Peek();
                    head.Dequeue();
                    if (head.Count == 0)
                    {
                        polylines.Dequeue();
                    }
                }
            }

            if (polylines.Count > 0)
            {
                CV.PolyLine(image, polylines.Select(ps => ps.ToArray()).ToArray(), false, Scalar.Rgb(255, 0, 0), 2);
            }

            CV.Circle(image, point, 3, Scalar.Rgb(0, 255, 0), 3);
            if (tracking == TrackingMode.None)
            {
                polylines.Clear();
                points = null;
            }
        }

        public override void Load(IServiceProvider provider)
        {
            points = new Queue<Point>(1);
            polylines = new Queue<Queue<Point>>(1);
            visualizer = (IplImageVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
            MouseEventHandler mouseHandler = (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (++tracking > TrackingMode.Fixed)
                    {
                        tracking = TrackingMode.None;
                    }
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

        enum TrackingMode
        {
            None,
            Infinite,
            Fixed
        }
    }
}
