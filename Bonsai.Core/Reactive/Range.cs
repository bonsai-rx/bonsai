using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an observable sequence of integral numbers within a specified range.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Generates an observable sequence of integral numbers within a specified range.")]
    public class Range : Source<int>
    {
        /// <summary>
        /// Gets or sets the value of the first integer in the sequence.
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the number of sequential integers to generate.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Generates an observable sequence of integral numbers within a specified range.
        /// </summary>
        /// <returns>
        /// An observable sequence that contains a range of sequential integral numbers.
        /// </returns>
        public override IObservable<int> Generate()
        {
            return Observable.Range(Start, Count);
        }
    }
}
