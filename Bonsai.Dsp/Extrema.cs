using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that finds the global minimum and maximum of each
    /// array in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Finds the global minimum and maximum of each array in the sequence.")]
    public class Extrema
    {
        static ArrayExtrema ProcessExtrema(Arr arr, Arr mask = null)
        {
            var extrema = new ArrayExtrema();
            CV.MinMaxLoc(arr,
                         out extrema.MinValue,
                         out extrema.MaxValue,
                         out extrema.MinLocation,
                         out extrema.MaxLocation,
                         mask);
            return extrema;
        }

        /// <summary>
        /// Finds the global minimum and maximum of each array in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ArrayExtrema"/> values, representing the minimum and
        /// maximum of the 2D array, and their corresponding locations.
        /// </returns>
        public IObservable<ArrayExtrema> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => ProcessExtrema(input));
        }

        /// <summary>
        /// Finds the global minimum and maximum of each array in an observable sequence,
        /// where each array is paired with a mask where non-zero values indicate which
        /// elements in the array should be considered when computing the extrema.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects over which to find the extrema.
        /// </typeparam>
        /// <typeparam name="TMask">
        /// The type of the array-like objects used as an operation mask.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of array pairs, where the first array contains the elements over
        /// which to find the global minimum and maximum, and the second array contains
        /// the operation mask, where non-zero values indicate which elements in the
        /// first array should be considered, and which should be ignored, in the computation.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ArrayExtrema"/> values, representing the minimum and
        /// maximum of the 2D array, and their corresponding locations.
        /// </returns>
        public IObservable<ArrayExtrema> Process<TArray, TMask>(IObservable<Tuple<TArray, TMask>> source)
            where TArray : Arr
            where TMask : Arr
        {
            return source.Select(input => ProcessExtrema(input.Item1, input.Item2));
        }
    }
}
