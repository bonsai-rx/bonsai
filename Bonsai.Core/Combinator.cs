using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Bonsai
{
    /// <summary>
    /// Represents a generic operation on observable sequences that preserves
    /// the sequence element type.
    /// </summary>
    [Combinator]
    [XmlType("CombinatorBase")]
    public abstract class Combinator
    {
        /// <summary>
        /// Processes the <paramref name="source"/> sequence into a new sequence of the
        /// same element type.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to process.</param>
        /// <returns>
        /// An observable sequence of the same data type as <paramref name="source"/>.
        /// </returns>
        public abstract IObservable<TSource> Process<TSource>(IObservable<TSource> source);
    }

    /// <summary>
    /// Represents a generic operation on observable sequences that returns
    /// another sequence of the specified element type.
    /// </summary>
    /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
    [Combinator]
    public abstract class Combinator<TResult>
    {
        /// <summary>
        /// Processes the <paramref name="source"/> sequence into a new sequence of the
        /// specified element type.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to process.</param>
        /// <returns>
        /// An observable sequence with elements of type <typeparamref name="TResult"/>.
        /// </returns>
        public abstract IObservable<TResult> Process<TSource>(IObservable<TSource> source);
    }

    /// <summary>
    /// Represents an operation on observable sequences of a specific element type.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
    [Combinator]
    public abstract class Combinator<TSource, TResult>
    {
        /// <summary>
        /// Processes the <paramref name="source"/> sequence into a new sequence of the
        /// specified element type.
        /// </summary>
        /// <param name="source">The source sequence to process.</param>
        /// <returns>
        /// An observable sequence with elements of type <typeparamref name="TResult"/>.
        /// </returns>
        public abstract IObservable<TResult> Process(IObservable<TSource> source);
    }
}
