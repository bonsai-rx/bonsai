using Bonsai;
using Bonsai.Design;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(PointOverlay), Target = typeof(MashupSource<ImageMashupVisualizer, PointVisualizer>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that overlays a sequence of points over an
    /// existing image visualizer.
    /// </summary>
    public class PointOverlay : DialogTypeVisualizer
    {
        Queue<Point?> points;
        ImageMashupVisualizer visualizer;
        IDisposable subscription;

        /// <summary>
        /// Gets or sets a value specifying the tracking mode used to overlay the
        /// point sequence on the image visualizer.
        /// </summary>
        public TrackingMode Tracking { get; set; }

        /// <summary>
        /// Gets or sets a value specifying how many previous points to include
        /// in the point sequence.
        /// </summary>
        public int Capacity { get; set; } = 1;

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
            if (Tracking != TrackingMode.None)
            {
                points.Enqueue(point);
                if (Tracking == TrackingMode.Rolling)
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
            var preloadCapacity = Capacity;
            points = new Queue<Point?>(preloadCapacity);
            while (preloadCapacity-- > 0) points.Enqueue(null);

            visualizer = (ImageMashupVisualizer)provider.GetService(typeof(MashupVisualizer));
            MouseEventHandler mouseHandler = (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (++Tracking > TrackingMode.Rolling)
                    {
                        Tracking = TrackingMode.None;
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
                Capacity = Tracking == TrackingMode.Rolling ? points.Count : 1;
                subscription = null;
                visualizer = null;
                points = null;
            }
        }
    }

    /// <summary>
    /// Specifies the tracking mode used to overlay a point sequence over an existing
    /// image visualizer.
    /// </summary>
    public enum TrackingMode
    {
        /// <summary>
        /// Specifies that only the current point should be overlaid on the image.
        /// </summary>
        None,

        /// <summary>
        /// Specifies that all recorded points should be overlaid as an infinite
        /// trace on the image.
        /// </summary>
        Infinite,

        /// <summary>
        /// Specifies that a fixed rolling number of the latest points should be
        /// overlaid on the image.
        /// </summary>
        Rolling
    }
}
