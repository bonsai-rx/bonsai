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
    /// Represents a combinator that projects each element of an observable sequence into zero
    /// or more windows based on element count information.
    /// </summary>
    [DefaultProperty("Count")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into zero or more windows based on element count information.")]
    public class ElementCountWindow : WindowCombinator
    {
        /// <summary>
        /// Gets or sets the maximum number of elements in each window.
        /// </summary>
        [Description("The maximum number of elements in each window.")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip between the creation of each window.
        /// If it is not specified, <see cref="Skip"/> will be equal to <see cref="Count"/>
        /// in order to generate consecutive non-overlapping windows.
        /// </summary>
        [Description("The optional number of elements to skip between the creation of each window.")]
        public int? Skip { get; set; }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more windows
        /// based on element count information.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <returns>An observable sequence of windows.</returns>
        public override IObservable<IObservable<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            var skip = Skip;
            if (skip.HasValue) return source.Window(Count, skip.Value);
            else return source.Window(Count);
        }
    }
}
