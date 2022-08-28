using OpenCV.Net;
using System;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Provides an abstract base class for operators that transform sequences
    /// of array-like objects.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class ArrayTransform
    {
        /// <summary>
        /// When overridden in a derived class, returns a sequence of array-like
        /// objects where each element is a transformation of the corresponding
        /// array in the original sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of array-like objects to be transformed.
        /// </param>
        /// <returns>
        /// A sequence of the transformed array-like objects.
        /// </returns>
        public abstract IObservable<TArray> Process<TArray>(IObservable<TArray> source) where TArray : Arr;
    }
}
