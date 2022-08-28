using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that copies all points of each contour in the
    /// sequence to a new array.
    /// </summary>
    [Description("Copies all points of each contour in the sequence to a new array.")]
    public class ContourPoints : Transform<Contour, Point[]>
    {
        static readonly Point[] EmptyPoints = new Point[0];

        static Point[] GetPoints(Seq input)
        {
            if (input == null) return EmptyPoints;
            return input.ToArray<Point>();
        }

        /// <summary>
        /// Copies all points of each contour in an observable sequence to a new array.
        /// </summary>
        /// <param name="source">
        /// The sequence of contours from which to extract the points.
        /// </param>
        /// <returns>
        /// A sequence of arrays containing all points of each contour in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<Point[]> Process(IObservable<Contour> source)
        {
            return source.Select(GetPoints);
        }

        /// <summary>
        /// Copies all points of each contour in an observable sequence to a new array.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Seq"/> objects from which to extract the points.
        /// </param>
        /// <returns>
        /// A sequence of arrays containing all points of each contour in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Point[]> Process(IObservable<Seq> source)
        {
            return source.Select(GetPoints);
        }

        /// <summary>
        /// Copies all points of each contour in an observable sequence to a new array.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="ConnectedComponent"/> objects from which to
        /// extract the points.
        /// </param>
        /// <returns>
        /// A sequence of arrays containing all points of each contour in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Point[]> Process(IObservable<ConnectedComponent> source)
        {
            return source.Select(input => GetPoints(input.Contour));
        }
    }
}
