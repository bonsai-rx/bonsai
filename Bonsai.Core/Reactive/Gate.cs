using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that takes the single next element from the sequence every
    /// time the trigger produces an element.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Takes the single next element from the sequence every time the trigger produces an element.")]
    public class Gate : BinaryCombinator
    {
        /// <summary>
        /// Takes the single next element from the sequence every time the trigger
        /// produces an element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TOther">The type of the elements in the sequence of gate events.</typeparam>
        /// <param name="source">The observable sequence to be gated.</param>
        /// <param name="other">
        /// The sequence of gate events. Every time a new gate event is received, the single
        /// next element from <paramref name="source"/> is allowed to propagate.
        /// </param>
        /// <returns>The gated observable sequence.</returns>
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Gate(other);
        }
    }
}
