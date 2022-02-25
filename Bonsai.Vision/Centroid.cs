using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that computes the centroid of each set of points,
    /// image moments, or polygonal contour in the sequence.
    /// </summary>
    [Description("Computes the centroid of each set of points, image moments, or polygonal contour in the sequence.")]
    public class Centroid : Transform<Point[], Point2f>
    {
        static readonly Point2f InvalidCentroid = new Point2f(float.NaN, float.NaN);

        /// <summary>
        /// Computes the centroid of each array of points in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Point"/> arrays for which to compute the centroid.
        /// </param>
        /// <returns>
        /// A <see cref="Point2f"/> value representing the centroid of each of
        /// the array of points in the sequence.
        /// </returns>
        public override IObservable<Point2f> Process(IObservable<Point[]> source)
        {
            return source.Select(input =>
            {
                if (input.Length == 0) return InvalidCentroid;
                var sum = Point2f.Zero;
                for (int i = 0; i < input.Length; i++)
                {
                    sum.X += input[i].X;
                    sum.Y += input[i].Y;
                }

                return new Point2f(sum.X / input.Length, sum.Y / input.Length);
            });
        }

        /// <summary>
        /// Computes the centroid of each collection of points in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Point"/> collections for which to compute the centroid.
        /// </param>
        /// <returns>
        /// A <see cref="Point2f"/> value representing the centroid of each collection of
        /// points in the sequence.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<IEnumerable<Point>> source)
        {
            return source.Select(input =>
            {
                var count = 0;
                var sum = Point2f.Zero;
                foreach (var point in input)
                {
                    sum.X += point.X;
                    sum.Y += point.Y;
                    count++;
                }

                if (count == 0) return InvalidCentroid;
                else return new Point2f(sum.X / count, sum.Y / count);
            });
        }

        /// <summary>
        /// Computes the centroid of each array of points in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Point2f"/> arrays for which to compute the centroid.
        /// </param>
        /// <returns>
        /// A <see cref="Point2f"/> value representing the centroid of each of
        /// the array of points in the sequence.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<Point2f[]> source)
        {
            return source.Select(input =>
            {
                if (input.Length == 0) return InvalidCentroid;
                var sum = Point2f.Zero;
                for (int i = 0; i < input.Length; i++)
                {
                    sum.X += input[i].X;
                    sum.Y += input[i].Y;
                }

                return new Point2f(sum.X / input.Length, sum.Y / input.Length);
            });
        }

        /// <summary>
        /// Computes the centroid of each collection of points in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Point2f"/> collections for which to compute the centroid.
        /// </param>
        /// <returns>
        /// A <see cref="Point2f"/> value representing the centroid of each collection of
        /// points in the sequence.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<IEnumerable<Point2f>> source)
        {
            return source.Select(input =>
            {
                var count = 0;
                var sum = Point2f.Zero;
                foreach (var point in input)
                {
                    sum.X += point.X;
                    sum.Y += point.Y;
                    count++;
                }

                if (count == 0) return InvalidCentroid;
                else return new Point2f(sum.X / count, sum.Y / count);
            });
        }

        static Point2f FromMoments(Moments moments)
        {
            if (moments.M00 > 0)
            {
                var x = moments.M10 / moments.M00;
                var y = moments.M01 / moments.M00;
                return new Point2f((float)x, (float)y);
            }
            else return InvalidCentroid;
        }

        /// <summary>
        /// Computes the centroid of each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to compute the centroid, where each
        /// pixel is weighed according to its intensity value.
        /// </param>
        /// <returns>
        /// A <see cref="Point2f"/> value representing the centroid of each image
        /// in the sequence.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var moments = new Moments(input);
                return FromMoments(moments);
            });
        }

        /// <summary>
        /// Computes the centroid of each polygonal contour in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Contour"/> objects for which to compute the centroid.
        /// </param>
        /// <returns>
        /// A <see cref="Point2f"/> value representing the centroid of each polygonal
        /// contour in the sequence.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<Contour> source)
        {
            return source.Select(input =>
            {
                var moments = new Moments(input);
                return FromMoments(moments);
            });
        }

        /// <summary>
        /// Extracts the centroid of each connected component in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="ConnectedComponent"/> objects for which to
        /// extract the centroid.
        /// </param>
        /// <returns>
        /// A <see cref="Point2f"/> value representing the centroid of each connected
        /// component in the sequence.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<ConnectedComponent> source)
        {
            return source.Select(input => input.Centroid);
        }
    }
}
