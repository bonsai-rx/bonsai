using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that repeats an observable sequence indefinitely.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Repeats the observable sequence indefinitely.")]
    public class Repeat : Combinator
    {
        /// <summary>
        /// Repeats the observable sequence indefinitely.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The observable sequence to repeat.</param>
        /// <returns>
        /// The observable sequence producing the elements of the given sequence repeatedly
        /// and sequentially.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Repeat();
        }
    }
}
