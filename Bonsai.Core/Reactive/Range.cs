using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that generates an observable sequence of integer
    /// numbers within a specified range.
    /// </summary>
    [DefaultProperty(nameof(Count))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Generates an observable sequence of integer numbers within a specified range.")]
    public class Range : Source<int>
    {
        /// <summary>
        /// Gets or sets the value of the first integer in the sequence.
        /// </summary>
        [Description("The value of the first integer in the sequence.")]
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the number of sequential integers to generate.
        /// </summary>
        [Description("The number of sequential integers to generate.")]
        public int Count { get; set; }

        /// <summary>
        /// Generates an observable sequence of integer numbers within a specified range.
        /// </summary>
        /// <returns>
        /// An observable sequence that contains a range of sequential integer numbers.
        /// </returns>
        public override IObservable<int> Generate()
        {
            return Observable.Range(Start, Count);
        }

        /// <summary>
        /// Generates an observable sequence of integer numbers within a specified range
        /// whenever the source sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence used to generate the range.</param>
        /// <returns>
        /// An observable sequence that generates a range of sequential integer numbers
        /// whenever the <paramref name="source"/> emits a notification.
        /// </returns>
        public IObservable<int> Generate<TSource>(IObservable<TSource> source)
        {
            var range = Enumerable.Range(Start, Count);
            return source.SelectMany(x => range);
        }
    }
}
