using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that counts all the non-zero elements for each array
    /// in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Counts all the non-zero elements for each array in the sequence.")]
    public class CountNonZero
    {
        /// <summary>
        /// Counts all the non-zero elements for each array in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of array-like objects for which to count non-zero elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="int"/> values representing the number of
        /// non-zero elements in each array.
        /// </returns>
        public IObservable<int> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(CV.CountNonZero);
        }
    }
}
