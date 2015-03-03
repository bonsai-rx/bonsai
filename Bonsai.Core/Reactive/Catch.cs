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
    /// Represents a combinator that continues an observable sequence that is terminated
    /// by an exception with the next observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Continues an observable sequence that is terminated by an exception with the next observable sequence.")]
    public class Catch
    {
        /// <summary>
        /// Continues an observable sequence that is terminated by an exception with
        /// the next observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source and handler sequences.</typeparam>
        /// <param name="first">The first observable sequence whose exception (if any) is caught.</param>
        /// <param name="second">
        /// The second observable sequence used to produce results when the first sequence
        /// terminates exceptionally.
        /// </param>
        /// <returns>
        /// An observable sequence containing the first sequence's elements, followed
        /// by the elements of the second sequence in case an exception occurred.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> first, IObservable<TSource> second)
        {
            return first.Catch(second);
        }

        /// <summary>
        /// Continues an observable sequence that is terminated by an exception with
        /// the next observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source and handler sequences.</typeparam>
        /// <param name="sources">The observable sequences to catch exceptions for.</param>
        /// <returns>
        /// An observable sequence containing elements from consecutive source sequences
        /// until a source sequence terminates successfully.
        /// </returns>
        public IObservable<TSource> Process<TSource>(params IObservable<TSource>[] sources)
        {
            return Observable.Catch(sources);
        }
    }
}
