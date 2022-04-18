using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that transforms a sequence of observable sequences into a
    /// sequence of values produced only from the most recent observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Transforms a sequence of observable sequences into a sequence of values produced only from the most recent observable sequence.")]
    public class Switch
    {
        /// <summary>
        /// Transforms a sequence of observable sequences into a sequence of values
        /// produced only from the most recent observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="source">A sequence of observable sequences.</param>
        /// <returns>
        /// An observable sequence that at any point produces values only from the
        /// most recent observable sequence that has been received.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<IObservable<TSource>> source)
        {
            return source.Switch();
        }
    }
}
