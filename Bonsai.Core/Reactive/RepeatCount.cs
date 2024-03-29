﻿using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that repeats an observable sequence a specified number of times.
    /// </summary>
    [DefaultProperty(nameof(Count))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Repeats the observable sequence a specified number of times.")]
    public class RepeatCount : Combinator
    {
        /// <summary>
        /// Gets or sets the number of times to repeat the sequence.
        /// </summary>
        [Description("The number of times the sequence should be repeated.")]
        public int Count { get; set; }

        /// <summary>
        /// Repeats the observable sequence a specified number of times.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The observable sequence to repeat.</param>
        /// <returns>
        /// The observable sequence producing the elements of the given sequence repeatedly.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Repeat(Count);
        }
    }
}
