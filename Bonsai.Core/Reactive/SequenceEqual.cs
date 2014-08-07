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
    /// Represents a combinator which determines whether two sequences are equal by comparing
    /// the elements pairwise.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Determines whether two sequences are equal by comparing the elements pairwise.")]
    public class SequenceEqual
    {
        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements pairwise.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="first">First observable sequence.</param>
        /// <param name="second">Second observable sequence.</param>
        /// <returns>
        /// An observable sequence that contains a single element which indicates whether
        /// both sequences are of equal length and their corresponding elements are equal
        /// according to the default equality comparer for their type.
        /// </returns>
        public IObservable<bool> Process<TSource>(IObservable<TSource> first, IObservable<TSource> second)
        {
            return first.SequenceEqual(second);
        }
    }
}
