using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that records the zero-based index of elements produced
    /// by an observable sequence.
    /// </summary>
    [Obsolete]
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Records the zero-based index of elements produced by an observable sequence.")]
    public class Index
    {
        /// <summary>
        /// Records the zero-based index of elements produced by an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence for which to record element indices.</param>
        /// <returns>An observable sequence with index information on elements.</returns>
        public IObservable<ElementIndex<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select((value, index) => new ElementIndex<TSource>(value, index));
        }
    }
}
