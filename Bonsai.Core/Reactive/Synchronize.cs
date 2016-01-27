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
    /// Represents a combinator that synchronizes the observable sequence
    /// such that observer notifications cannot be delivered concurrently.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Synchronizes the observable sequence such that observer notifications cannot be delivered concurrently.")]
    public class Synchronize : Combinator
    {
        /// <summary>
        /// Synchronizes the observable sequence such that observer notifications
        /// cannot be delivered concurrently.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The observable sequence to synchronize.</param>
        /// <returns>
        /// The source sequence whose outgoing calls to observers are synchronized.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Synchronize();
        }
    }
}
