using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that bypasses the specified number of elements at the end
    /// of an observable sequence.
    /// </summary>
    [DefaultProperty("Count")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Bypasses the specified number of contiguous elements at the end of the sequence.")]
    public class SkipLast : Combinator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkipLast"/> class.
        /// </summary>
        public SkipLast()
        {
            Count = 1;
        }

        /// <summary>
        /// Gets or sets the number of elements to skip at the end of the sequence.
        /// </summary>
        [Description("The number of elements to skip at the end of the sequence.")]
        public int Count { get; set; }

        /// <summary>
        /// Bypasses the specified number of elements at the end of an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence to take elements from.</param>
        /// <returns>
        /// An observable sequence containing the source sequence elements except for
        /// the bypassed ones at the end.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.SkipLast(Count);
        }
    }
}
