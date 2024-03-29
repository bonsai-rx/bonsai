﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that creates an array containing every element
    /// in the observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Creates an array containing every element in the observable sequence.")]
    public class ToArray
    {
        /// <summary>
        /// Creates an array containing every element in the observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to get an array of elements for.</param>
        /// <returns>
        /// An observable sequence containing a single element with the array of all elements
        /// in the source sequence.
        /// </returns>
        public IObservable<TSource[]> Process<TSource>(IObservable<TSource> source)
        {
            return source.ToArray();
        }
    }
}
