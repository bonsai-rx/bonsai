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
    /// Represents a combinator that returns the first element of an observable sequence.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns the first element of an observable sequence.")]
    public class First : Combinator
    {
        /// <summary>
        /// Returns the first element of an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The sequence to take the first element from.</param>
        /// <returns>
        /// An observable sequence with a single element that contains the first element
        /// of the observable sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.FirstAsync();
        }
    }
}
