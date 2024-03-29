﻿using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that repeats an observable sequence the specified
    /// number of times or until it successfully terminates.
    /// </summary>
    [DefaultProperty(nameof(Count))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Repeats the observable sequence the specified number of times or until it successfully terminates.")]
    public class RetryCount : Combinator
    {
        /// <summary>
        /// Gets or sets the number of times to repeat the sequence.
        /// </summary>
        [Description("The number of times to repeat the sequence.")]
        public int Count { get; set; }

        /// <summary>
        /// Repeats the observable sequence the specified number of times
        /// or until it successfully terminates.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The observable sequence to repeat until it successfully terminates.</param>
        /// <returns>
        /// The observable sequence producing the elements of the given sequence repeatedly
        /// until it terminates successfully.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Retry(Count);
        }
    }
}
