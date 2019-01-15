using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Calculates whether a point is contained inside a rectangle, contour, or other polygonal shape.")]
    public class ContainsPoint
    {
        static bool Contains(Rect rect, Point point)
        {
            return point.X >= rect.X && point.X < (rect.X + rect.Width) &&
                   point.Y >= rect.Y && point.Y < (rect.Y + rect.Height);
        }

        static bool Contains(Contour contour, Point2f point)
        {
            return contour != null && CV.PointPolygonTest(contour, point, false) > 0;
        }

        static bool Contains(Point[] contour, Point2f point)
        {
            if (contour == null) return false;
            using (var contourHeader = Mat.CreateMatHeader(contour, contour.Length, 2, Depth.S32, 1))
            {
                return CV.PointPolygonTest(contourHeader, point, false) > 0;
            }
        }

        static bool Contains(Point[][] contour, Point2f point)
        {
            if (contour == null) return false;
            for (int i = 0; i < contour.Length; i++)
            {
                if (Contains(contour[i], point))
                {
                    return true;
                }
            }

            return false;
        }

        public IObservable<bool> Process(IObservable<Tuple<Rect, Point>> source)
        {
            return source.Select(input => Contains(input.Item1, input.Item2));
        }

        public IObservable<bool> Process(IObservable<Tuple<Rect, Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1, new Point(input.Item2)));
        }

        public IObservable<bool> Process(IObservable<Tuple<Contour, Point>> source)
        {
            return source.Select(input => Contains(input.Item1, new Point2f(input.Item2)));
        }

        public IObservable<bool> Process(IObservable<Tuple<Contour, Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1, input.Item2));
        }

        public IObservable<bool> Process(IObservable<Tuple<ConnectedComponent, Point>> source)
        {
            return source.Select(input => Contains(input.Item1.Contour, new Point2f(input.Item2)));
        }

        public IObservable<bool> Process(IObservable<Tuple<ConnectedComponent, Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1.Contour, input.Item2));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point[], Point>> source)
        {
            return source.Select(input => Contains(input.Item1, new Point2f(input.Item2)));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point[], Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1, input.Item2));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point[][], Point>> source)
        {
            return source.Select(input => Contains(input.Item1, new Point2f(input.Item2)));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point[][], Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1, input.Item2));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point, Rect>> source)
        {
            return source.Select(input => Contains(input.Item2, input.Item1));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point2f, Rect>> source)
        {
            return source.Select(input => Contains(input.Item2, new Point(input.Item1)));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point, Contour>> source)
        {
            return source.Select(input => Contains(input.Item2, new Point2f(input.Item1)));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point2f, Contour>> source)
        {
            return source.Select(input => Contains(input.Item2, input.Item1));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point, ConnectedComponent>> source)
        {
            return source.Select(input => Contains(input.Item2.Contour, new Point2f(input.Item1)));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point2f, ConnectedComponent>> source)
        {
            return source.Select(input => Contains(input.Item2.Contour, input.Item1));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point, Point[]>> source)
        {
            return source.Select(input => Contains(input.Item2, new Point2f(input.Item1)));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point2f, Point[]>> source)
        {
            return source.Select(input => Contains(input.Item2, input.Item1));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point, Point[][]>> source)
        {
            return source.Select(input => Contains(input.Item2, new Point2f(input.Item1)));
        }

        public IObservable<bool> Process(IObservable<Tuple<Point2f, Point[][]>> source)
        {
            return source.Select(input => Contains(input.Item2, input.Item1));
        }
    }
}
