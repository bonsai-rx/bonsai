using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that extracts a range of elements from an observable sequence.
    /// </summary>
    [DefaultProperty("Step")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Extracts a range of elements from an observable sequence.")]
    public class Slice : Combinator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Slice"/> class.
        /// </summary>
        public Slice()
        {
            Step = 1;
        }

        /// <summary>
        /// Gets or sets the element index at which the slice begins.
        /// </summary>
        [Description("The element index at which the slice begins.")]
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip between slice elements.
        /// </summary>
        [Description("The number of elements to skip between slice elements.")]
        public int Step { get; set; }

        /// <summary>
        /// Gets or sets the optional element index at which the slice ends.
        /// </summary>
        [Description("The optional element index at which the slice ends.")]
        public int? Stop { get; set; }

        /// <summary>
        /// Extracts a range of elements from an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to slice.</param>
        /// <returns>The sliced sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                int i = 0;
                return source.Where(xs =>
                {
                    var index = i++;
                    return index >= Start && (!Stop.HasValue || index < Stop) && (index - Start) % Step == 0;
                });
            });
        }
    }
}
