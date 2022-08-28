using OpenCV.Net;
using System;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Provides an abstract base class for operators that perform a binary transformation
    /// on pairs of array-like objects in an observable sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class BinaryArrayTransform
    {
        /// <summary>
        /// When overridden in a derived class, applies a binary transformation to all
        /// pairs of array-like objects in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence containing the pairs of array-like objects for which to apply
        /// the binary transformation.
        /// </param>
        /// <returns>
        /// A sequence containing the results of the binary transformation.
        /// </returns>
        public abstract IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source) where TArray : Arr;
    }
}
