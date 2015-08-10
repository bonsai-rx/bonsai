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
    /// Represents a combinator that determines whether the observable
    /// sequence is empty.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Determines whether the observable sequence is empty.")]
    public class IsEmpty
    {
        /// <summary>
        /// Determines whether the observable sequence is empty.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to check.</param>
        /// <returns>
        /// An observable sequence containing a single element determining whether
        /// the source sequence is empty.
        /// </returns>
        public IObservable<bool> Process<TSource>(IObservable<TSource> source)
        {
            return source.IsEmpty();
        }
    }
}
