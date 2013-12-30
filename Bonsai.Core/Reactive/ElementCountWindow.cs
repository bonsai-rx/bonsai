using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml;
using System.Linq.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that projects each element of an observable sequence into consecutive
    /// non-overlapping windows with the specified maximum number of elements.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into non-overlapping windows with the specified maximum number of elements.")]
    public class ElementCountWindow : WindowCombinator
    {
        /// <summary>
        /// Gets or sets the maximum number of elements in each window.
        /// </summary>
        [Description("The maximum number of elements in each window.")]
        public int Count { get; set; }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping
        /// windows with the specified maximum number of elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <returns>An observable sequence of windows.</returns>
        public override IObservable<IObservable<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Window(Count);
        }
    }
}
