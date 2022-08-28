using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that calculates the absolute array norm, absolute difference norm,
    /// or relative difference norm for each array in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Calculates the absolute array norm, absolute difference norm, or relative difference norm for each array in the sequence.")]
    public class Norm
    {
        /// <summary>
        /// Gets or sets the type of array norm to calculate.
        /// </summary>
        [Description("The type of array norm to calculate.")]
        public NormTypes NormType { get; set; } = NormTypes.L2;

        static double ComputeNorm(double x, double y, NormTypes norm)
        {
            switch (norm)
            {
                case NormTypes.C: return Math.Max(Math.Abs(x), Math.Abs(y));
                case NormTypes.L1: return Math.Abs(x) + Math.Abs(y);
                case NormTypes.L2: return Math.Sqrt(x * x + y * y);
                case NormTypes.L2Sqr: return x * x + y * y;
                default: throw new InvalidOperationException("The specified norm is not supported for this data type.");
            }
        }

        static double ComputeNorm(double x, double y, double z, NormTypes norm)
        {
            switch (norm)
            {
                case NormTypes.C: return Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
                case NormTypes.L1: return Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
                case NormTypes.L2: return Math.Sqrt(x * x + y * y + z * z);
                case NormTypes.L2Sqr: return x * x + y * y + z * z;
                default: throw new InvalidOperationException("The specified norm is not supported for this data type.");
            }
        }

        /// <summary>
        /// Calculates the absolute norm for each 2D point in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D points with integer coordinates for which to calculate
        /// the norm.
        /// </param>
        /// <returns>
        /// A sequence containing the absolute norm for each point in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<Point> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, NormType));
        }

        /// <summary>
        /// Calculates the absolute norm for each 2D point in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D points with single-precision floating-point coordinates
        /// for which to calculate the norm.
        /// </param>
        /// <returns>
        /// A sequence containing the absolute norm for each point in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<Point2f> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, NormType));
        }

        /// <summary>
        /// Calculates the absolute norm for each 2D point in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D points with double-precision floating-point coordinates
        /// for which to calculate the norm.
        /// </param>
        /// <returns>
        /// A sequence containing the absolute norm for each point in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<Point2d> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, NormType));
        }

        /// <summary>
        /// Calculates the absolute norm for each 3D point in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 3D points with single-precision floating-point coordinates
        /// for which to calculate the norm.
        /// </param>
        /// <returns>
        /// A sequence containing the absolute norm for each point in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<Point3f> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, input.Z, NormType));
        }

        /// <summary>
        /// Calculates the absolute norm for each 3D point in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 3D points with double-precision floating-point coordinates
        /// for which to calculate the norm.
        /// </param>
        /// <returns>
        /// A sequence containing the absolute norm for each point in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<Point3d> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, input.Z, NormType));
        }

        /// <summary>
        /// Calculates the absolute array norm for each array in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-dimensional arrays for which to calculate the norm.
        /// </param>
        /// <returns>
        /// A sequence containing the absolute norm for each array in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => CV.Norm(input, null, NormType));
        }

        /// <summary>
        /// Calculates the absolute difference norm, or relative difference norm, between each
        /// pair of arrays in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray1">
        /// The type of the first array-like object.
        /// </typeparam>
        /// <typeparam name="TArray2">
        /// The type of the second array-like object.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of multi-dimensional arrays for which to calculate the
        /// absolute difference norm or relative difference norm.
        /// </param>
        /// <returns>
        /// A sequence containing the absolute difference norm, or relative difference norm,
        /// between each pair of arrays in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double> Process<TArray1, TArray2>(IObservable<Tuple<TArray1, TArray2>> source)
            where TArray1 : Arr
            where TArray2 : Arr
        {
            return source.Select(input => CV.Norm(input.Item1, input.Item2, NormType));
        }

        /// <summary>
        /// Calculates the absolute difference norm, or relative difference norm, between each
        /// pair of arrays in an observable sequence with an additional operation mask.
        /// </summary>
        /// <typeparam name="TArray1">
        /// The type of the first array-like object.
        /// </typeparam>
        /// <typeparam name="TArray2">
        /// The type of the second array-like object.
        /// </typeparam>
        /// <typeparam name="TMask">
        /// The type of the array-like objects used in the operation mask.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of multi-dimensional arrays for which to calculate the
        /// absolute difference norm or relative difference norm, for elements in
        /// which the operation mask is non-zero.
        /// </param>
        /// <returns>
        /// A sequence containing the absolute difference norm, or relative difference norm,
        /// between each pair of arrays in the <paramref name="source"/> sequence, for
        /// elements in which the operation mask is non-zero.
        /// </returns>
        public IObservable<double> Process<TArray1, TArray2, TMask>(IObservable<Tuple<TArray1, TArray2, TMask>> source)
            where TArray1 : Arr
            where TArray2 : Arr
            where TMask : Arr
        {
            return source.Select(input => CV.Norm(input.Item1, input.Item2, NormType, input.Item3));
        }
    }
}
