using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that calculates the sum of each channel for all the
    /// arrays in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Calculates the sum of each channel for all the arrays in the sequence.")]
    public class Sum
    {
        /// <summary>
        /// Calculates the sum of each channel for all the arrays in an observable
        /// sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of array values for which to calculate the per-channel sum.
        /// </param>
        /// <returns>
        /// A <see cref="Scalar"/> tuple containing the sum of each channel for all
        /// the arrays in the <paramref name="source"/> sequence.
        /// </returns>
        /// <remarks>
        /// For multi-channel images using BGR, RGB, or other color formats, the order of
        /// values in the <see cref="Scalar"/> tuple follows the order of channels in the
        /// color format, e.g. for a BGR image, the sum for the blue-channel will be
        /// stored in <see cref="Scalar.Val0"/>, the sum for the green-channel in
        /// <see cref="Scalar.Val1"/>, etc.
        /// 
        /// For single-channel arrays such as a grayscale image or a 2D floating point
        /// array with signal processing data, the sum will be stored in the first
        /// value of the tuple, <see cref="Scalar.Val0"/>.
        /// </remarks>
        public IObservable<Scalar> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(CV.Sum);
        }
    }
}
