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
    /// Represents a combinator that concatenates any number of observable sequences even if any of
    /// the sequences terminates exceptionally.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Concatenates any number of observable sequences even if any of the sequences terminates exceptionally.")]
    public class OnErrorResumeNext
    {
        /// <summary>
        /// Concatenates the second observable sequence to the first observable sequence upon
        /// successful or exceptional termination of the first.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the first sequence.</typeparam>
        /// <param name="first">The first observable sequence.</param>
        /// <param name="second">The second observable sequence.</param>
        /// <returns>
        /// An observable sequence that concatenates the first and second sequence, even
        /// if the first sequence terminates exceptionally.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> first, IObservable<TSource> second)
        {
            return first.OnErrorResumeNext(second);
        }

        /// <summary>
        /// Concatenates all of the specified observable sequences, even if the previous
        /// observable sequence terminated exceptionally.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="sources">The observable sequences to concatenate.</param>
        /// <returns>
        /// An observable sequence that concatenates the source sequences, even if a
        /// sequence terminates exceptionally.
        /// </returns>
        public IObservable<TSource> Process<TSource>(params IObservable<TSource>[] sources)
        {
            return Observable.OnErrorResumeNext(sources);
        }
    }
}
