using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(PointMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, PointVisualizer>))]

namespace Bonsai.Vision.Design
{
    public class PointMashupVisualizer : MashupTypeVisualizer
    {
        TrackingMode tracking;
        Queue<Point> points;
        Queue<Queue<Point>> polylines;
        ImageMashupVisualizer visualizer;
        IDisposable subscription;

        public override void Show(object value)
        {
            Point? point;
            if (value is Point)
            {
                point = (Point)value;
            }
            else if (value is Point2f point2f)
            {
                if (float.IsNaN(point2f.X) || float.IsNaN(point2f.Y)) point = null;
                else point = new Point(point2f);
            }
            else
            {
                var point2d = (Point2d)value;
                if (double.IsNaN(point2d.X) || double.IsNaN(point2d.Y)) point = null;
                else point = new Point((int)point2d.X, (int)point2d.Y);
            }

            var image = visualizer.VisualizerImage;
            if (!point.HasValue)
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

                points.Enqueue(point.Value);
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

            if (point.HasValue)
            {
                CV.Circle(image, point.Value, 3, Scalar.Rgb(0, 255, 0), 3);
            }

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
            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(DialogMashupVisualizer));
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
