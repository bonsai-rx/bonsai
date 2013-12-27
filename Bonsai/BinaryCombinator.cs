using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Represents a generic operation on two observable sequences where the elements of the first
    /// sequence are propagated based on notifications from the second sequence.
    /// </summary>
    [Combinator]
    public abstract class BinaryCombinator
    {
        /// <summary>
        /// Processes the <paramref name="source"/> sequence into a new sequence of the
        /// same element type based on notifications from the <paramref name="other"/> sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the first sequence.</typeparam>
        /// <typeparam name="TOther">The type of the elements in the other sequence.</typeparam>
        /// <param name="source">The source sequence to process.</param>
        /// <param name="other">
        /// Observable sequence which controls propagation of the <paramref name="source"/> sequence.
        /// </param>
        /// <returns>
        /// An observable sequence of the same data type as <paramref name="source"/>.
        /// </returns>
        public abstract IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other);
    }
}
