using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that determines whether each point in the sequence is
    /// contained inside a rectangle, contour, or other polygonal shape.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Determines whether each point in the sequence is contained inside a rectangle, contour, or other polygonal shape.")]
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

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside a rectangle.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a rectangle and a point with integer
        /// coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the rectangle.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Rect, Point>> source)
        {
            return source.Select(input => Contains(input.Item1, input.Item2));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside a rectangle.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a rectangle and a point with single-precision
        /// floating-point coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the rectangle.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Rect, Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1, new Point(input.Item2)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside a polygonal contour.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a <see cref="Contour"/> object and a
        /// point with integer coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Contour, Point>> source)
        {
            return source.Select(input => Contains(input.Item1, new Point2f(input.Item2)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside a polygonal contour.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a <see cref="Contour"/> object and a
        /// point with single-precision floating-point coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Contour, Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1, input.Item2));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside the polygonal contour of a connected component.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a <see cref="ConnectedComponent"/> object
        /// and a point with integer coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour of a connected component.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<ConnectedComponent, Point>> source)
        {
            return source.Select(input => Contains(input.Item1.Contour, new Point2f(input.Item2)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside the polygonal contour of a connected component.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a <see cref="ConnectedComponent"/> object
        /// and a point with single-precision floating-point coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour of a connected component.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<ConnectedComponent, Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1.Contour, input.Item2));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside the polygonal contour specified by an array of vertices.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a <see cref="Point"/> array specifying the
        /// contour and a point with integer coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour specified by an array of vertices.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point[], Point>> source)
        {
            return source.Select(input => Contains(input.Item1, new Point2f(input.Item2)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside the polygonal contour specified by an array of vertices.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a <see cref="Point"/> array specifying the
        /// contour and a point with single-precision floating-point coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour specified by an array of vertices.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point[], Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1, input.Item2));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside any of the polygonal contours specified by arrays of vertices.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a jagged array of <see cref="Point"/> values
        /// specifying the contours and a point with integer coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside any of the polygonal contours specified by arrays of
        /// vertices.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point[][], Point>> source)
        {
            return source.Select(input => Contains(input.Item1, new Point2f(input.Item2)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside any of the polygonal contours specified by arrays of vertices.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a jagged array of <see cref="Point"/> values
        /// specifying the contours and a point with single-precision floating-point
        /// coordinates to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside any of the polygonal contours specified by arrays of
        /// vertices.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point[][], Point2f>> source)
        {
            return source.Select(input => Contains(input.Item1, input.Item2));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside a rectangle.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with integer coordinates and
        /// a rectangle to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the rectangle.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point, Rect>> source)
        {
            return source.Select(input => Contains(input.Item2, input.Item1));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside a rectangle.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with single-precision
        /// floating-point coordinates and a rectangle to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the rectangle.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point2f, Rect>> source)
        {
            return source.Select(input => Contains(input.Item2, new Point(input.Item1)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside a polygonal contour.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with integer coordinates and
        /// a <see cref="Contour"/> object to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point, Contour>> source)
        {
            return source.Select(input => Contains(input.Item2, new Point2f(input.Item1)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside a polygonal contour.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with single-precision
        /// floating-point coordinates and a <see cref="Contour"/> object to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point2f, Contour>> source)
        {
            return source.Select(input => Contains(input.Item2, input.Item1));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside the polygonal contour of a connected component.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with integer coordinates and
        /// a <see cref="ConnectedComponent"/> object to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour of a connected component.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point, ConnectedComponent>> source)
        {
            return source.Select(input => Contains(input.Item2.Contour, new Point2f(input.Item1)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside the polygonal contour of a connected component.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with single-precision
        /// floating-point coordinates and a <see cref="ConnectedComponent"/> object
        /// to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour of a connected component.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point2f, ConnectedComponent>> source)
        {
            return source.Select(input => Contains(input.Item2.Contour, input.Item1));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside the polygonal contour specified by an array of vertices.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with integer coordinates and
        /// a <see cref="Point"/> array specifying the contour to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour specified by an array of vertices.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point, Point[]>> source)
        {
            return source.Select(input => Contains(input.Item2, new Point2f(input.Item1)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside the polygonal contour specified by an array of vertices.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with single-precision
        /// floating-point coordinates and a <see cref="Point"/> array specifying
        /// the contour to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside the polygonal contour specified by an array of vertices.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point2f, Point[]>> source)
        {
            return source.Select(input => Contains(input.Item2, input.Item1));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside any of the polygonal contours specified by arrays of vertices.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with integer coordinates and a
        /// jagged array of <see cref="Point"/> values specifying the contours to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside any of the polygonal contours specified by arrays of
        /// vertices.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point, Point[][]>> source)
        {
            return source.Select(input => Contains(input.Item2, new Point2f(input.Item1)));
        }

        /// <summary>
        /// Determines whether each point in an observable sequence is contained
        /// inside any of the polygonal contours specified by arrays of vertices.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing a point with single-precision floating-point
        /// coordinates and a jagged array of <see cref="Point"/> values specifying the
        /// contours to test.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="bool"/> values indicating whether the point
        /// is contained inside any of the polygonal contours specified by arrays of
        /// vertices.
        /// </returns>
        public IObservable<bool> Process(IObservable<Tuple<Point2f, Point[][]>> source)
        {
            return source.Select(input => Contains(input.Item2, input.Item1));
        }
    }
}
