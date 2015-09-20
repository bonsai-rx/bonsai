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
    /// Represents a combinator that returns the last element of an observable sequence,
    /// or a default value if no such element exists.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns the last element of an observable sequence, or a default value if no such element exists.")]
    public class LastOrDefault : Combinator
    {
        /// <summary>
        /// Returns the last element of an observable sequence, or a default value
        /// if no such element exists.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence to take the last element from.</param>
        /// <returns>
        /// An observable sequence with a single element that contains the last element
        /// of the observable sequence, or a default value if no such element exists.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.LastOrDefaultAsync();
        }
    }
}
