using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that computes the magnitude and angle of each array
    /// of 2D vectors in the sequence.
    /// </summary>
    [Description("Computes the magnitude and angle of each array of 2D vectors in the sequence.")]
    public class CartToPolar : ArrayTransform
    {
        /// <summary>
        /// Gets or sets a value specifying whether vector angle values are measured in degrees.
        /// </summary>
        [Description("Specifies whether vector angle values are measured in degrees.")]
        public bool AngleInDegrees { get; set; }

        /// <summary>
        /// Computes the magnitude and angle of each array of 2D vectors in the sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of 2D vector fields represented by a 2-channel array or image,
        /// for which to compute the magnitude and angle.
        /// </param>
        /// <returns>
        /// A sequence of 2-channel arrays or images, where the first channel of each
        /// element stores the magnitude and the second channel the angle of a 2D vector.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var channelFactory = ArrFactory<TArray>.TemplateSizeDepthFactory;
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var x = channelFactory(input, 1);
                var y = channelFactory(input, 1);
                var magnitude = channelFactory(input, 1);
                var angle = channelFactory(input, 1);
                var output = outputFactory(input);
                CV.Split(input, x, y, null, null);
                CV.CartToPolar(x, y, magnitude, angle, AngleInDegrees);
                CV.Merge(magnitude, angle, null, null, output);
                return output;
            });
        }

        /// <summary>
        /// Computes the magnitude and angle for each pair of cartesian coordinates in the sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of arrays, where the first array stores the x-coordinates, and the
        /// second array the y-coordinates of a 2D vector field for which to compute the magnitude
        /// and angle.
        /// </param>
        /// <returns>
        /// A sequence of pairs of arrays, where the first array stores the magnitude, and the second
        /// array stores the angle of a 2D vector.
        /// </returns>
        public IObservable<Tuple<TArray, TArray>> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
            where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var magnitude = outputFactory(input.Item1);
                var angle = outputFactory(input.Item1);
                CV.CartToPolar(input.Item1, input.Item2, magnitude, angle, AngleInDegrees);
                return Tuple.Create(magnitude, angle);
            });
        }

        void Process(double x, double y, out double magnitude, out double angle)
        {
            const double RadiansToDegrees = 180.0 / Math.PI;
            magnitude = Math.Sqrt(x * x + y * y);
            angle = Math.Atan2(y, x);
            if (AngleInDegrees)
            {
                angle *= RadiansToDegrees;
            }
        }

        /// <summary>
        /// Computes the magnitude and angle for each pair of 2D points in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D points with double-precision cartesian coordinates, for
        /// which to compute the corresponding polar coordinates.
        /// </param>
        /// <returns>
        /// A sequence of points specifying the corresponding double-precision polar
        /// coordinates for each 2D vector in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Point2d> Process(IObservable<Point2d> source)
        {
            return source.Select(input =>
            {
                Point2d polar;
                Process(input.X, input.Y, out polar.X, out polar.Y);
                return polar;
            });
        }

        /// <summary>
        /// Computes the magnitude and angle for each pair of 2D points in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D points with single-precision cartesian coordinates, for
        /// which to compute the corresponding polar coordinates.
        /// </param>
        /// <returns>
        /// A sequence of points specifying the corresponding single-precision polar
        /// coordinates for each 2D vector in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<Point2f> source)
        {
            return source.Select(input =>
            {
                Point2d polar;
                Process(input.X, input.Y, out polar.X, out polar.Y);
                return new Point2f((float)polar.X, (float)polar.Y);
            });
        }

        /// <summary>
        /// Computes the polar coordinates for each pair of cartesian coordinates in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs specifying double-precision cartesian coordinates, where
        /// the first item stores the x-coordinate, and the second item the y-coordinate
        /// of a 2D vector for which to compute the polar coordinates.
        /// </param>
        /// <returns>
        /// A sequence of pairs specifying double-precision polar coordinates, where the
        /// first item stores the magnitude, and the second item the angle of a 2D vector.
        /// </returns>
        public IObservable<Tuple<double, double>> Process(IObservable<Tuple<double, double>> source)
        {
            return source.Select(input =>
            {
                Process(input.Item1, input.Item2, out double magnitude, out double angle);
                return Tuple.Create(magnitude, angle);
            });
        }

        /// <summary>
        /// Computes the polar coordinates for each pair of cartesian coordinates in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs specifying single-precision cartesian coordinates, where
        /// the first item stores the x-coordinate, and the second item the y-coordinate
        /// of a 2D vector for which to compute the polar coordinates.
        /// </param>
        /// <returns>
        /// A sequence of pairs specifying single-precision polar coordinates, where the
        /// first item stores the magnitude, and the second item the angle of a 2D vector.
        /// </returns>
        public IObservable<Tuple<float, float>> Process(IObservable<Tuple<float, float>> source)
        {
            return source.Select(input =>
            {
                Process(input.Item1, input.Item2, out double magnitude, out double angle);
                return Tuple.Create((float)magnitude, (float)angle);
            });
        }
    }
}
