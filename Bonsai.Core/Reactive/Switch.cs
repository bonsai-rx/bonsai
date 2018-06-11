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
    /// Represents a combinator that transforms a sequence of windows into a sequence of values
    /// produced only from the most recent window.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Combines higher-order observables into a single sequence of values produced only from the most recent observable.")]
    public class Switch
    {
        /// <summary>
        /// Transforms a sequence of higher-order observables into a sequence of values produced only from
        /// the most recent observable.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence of higher-order observables to switch over.</param>
        /// <returns>
        /// An observable sequence of values produced only from the most recent observable.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<IObservable<TSource>> source)
        {
            return source.Switch();
        }
    }
}
