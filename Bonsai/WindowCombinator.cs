using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Represents a generic operation which projects each element of an observable sequence
    /// into a sequence of windows.
    /// </summary>
    [Combinator]
    public abstract class WindowCombinator
    {
        /// <summary>
        /// Projects each element of the <paramref name="source"/> sequence into a sequence of
        /// windows.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <returns>The sequence of windows.</returns>
        public abstract IObservable<IObservable<TSource>> Process<TSource>(IObservable<TSource> source);
    }
}
