using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that converts each array-like object in the sequence
    /// into an image.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts each array-like object in the sequence into an image.")]
    public class ConvertToImage
    {
        /// <summary>
        /// Converts each array-like object in an observable sequence into an image.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of array-like objects to be converted.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the
        /// image header for the arbitrary array.
        /// </returns>
        public IObservable<IplImage> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => input.GetImage());
        }
    }
}
