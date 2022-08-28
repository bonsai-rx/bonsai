using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that calculates the average, or arithmetic mean, of each channel for
    /// all the arrays in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Calculates the average, or arithmetic mean, of each channel for all the arrays in the sequence.")]
    public class Average
    {
        /// <summary>
        /// Calculates the average, or arithmetic mean, of each channel for all the arrays
        /// in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of array values for which to calculate the per-channel average.
        /// </param>
        /// <returns>
        /// A <see cref="Scalar"/> tuple containing the average of each channel for all
        /// the arrays in the <paramref name="source"/> sequence.
        /// </returns>
        /// <remarks>
        /// For multi-channel images using BGR, RGB, or other color formats, the order of
        /// values in the <see cref="Scalar"/> tuple follows the order of channels in the
        /// color format, e.g. for a BGR image, the average for the blue-channel will be
        /// stored in <see cref="Scalar.Val0"/>, the average for the green-channel in
        /// <see cref="Scalar.Val1"/>, etc.
        /// 
        /// For single-channel arrays such as a grayscale image or a 2D floating point
        /// array with signal processing data, the average will be stored in the first
        /// value of the tuple, <see cref="Scalar.Val0"/>.
        /// </remarks>
        public IObservable<Scalar> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => CV.Avg(input));
        }

        /// <summary>
        /// Calculates the average, or arithmetic mean, of each channel for all the arrays
        /// in an observable sequence, where each array is paired with a mask where non-zero
        /// values indicate which elements in the array should be averaged.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects to average.
        /// </typeparam>
        /// <typeparam name="TMask">
        /// The type of the array-like objects used as an operation mask.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of array pairs, where the first array contains the elements used
        /// to compute the average, and the second array contains the operation mask,
        /// where non-zero values indicate which elements in the first array should be averaged.
        /// </param>
        /// <returns>
        /// A <see cref="Scalar"/> tuple containing the average of each channel for all
        /// the arrays in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Scalar> Process<TArray, TMask>(IObservable<Tuple<TArray, TMask>> source)
            where TArray : Arr
            where TMask : Arr
        {
            return source.Select(input => CV.Avg(input.Item1, input.Item2));
        }
    }
}
