using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(PointMashupVisualizer), Target = typeof(VisualizerMashup<ImageMashupVisualizer, PointVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays a sequence of points over an
    /// existing image visualizer.
    /// </summary>
    public class PointMashupVisualizer : MashupTypeVisualizer
    {
        TrackingMode tracking;
        Queue<Point?> points;
        ImageMashupVisualizer visualizer;
        IDisposable subscription;

        /// <inheritdoc/>
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
            if (tracking != TrackingMode.None)
            {
                points.Enqueue(point);
                if (tracking == TrackingMode.Fixed)
                {
                    points.Dequeue();
                }
            }
            else points.Clear();

            if (points.Count > 0)
            {
                var segment = new List<Point>();
                var polyline = new List<Point[]>();
                foreach (var p in points)
                {
                    if (p.HasValue) segment.Add(p.Value);
                    else if (segment.Count > 0)
                    {
                        polyline.Add(segment.ToArray());
                        segment.Clear();
                    }
                }

                if (segment.Count > 0)
                {
                    polyline.Add(segment.ToArray());
                }

                CV.PolyLine(image, polyline.ToArray(), false, Scalar.Rgb(255, 0, 0), 2);
            }

            if (point.HasValue)
            {
                CV.Circle(image, point.Value, 3, Scalar.Rgb(0, 255, 0), 3);
            }
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            points = new Queue<Point?>(1);
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

        /// <inheritdoc/>
        public override void Unload()
        {
            if (subscription != null)
            {
                subscription.Dispose();
                subscription = null;
                visualizer = null;
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
