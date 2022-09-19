using System;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that projects each element of an observable sequence into zero
    /// or more windows based on element count information.
    /// </summary>
    [DefaultProperty(nameof(Count))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into zero or more windows based on element count information.")]
    public class WindowCount : WindowCombinator
    {
        /// <summary>
        /// Gets or sets the maximum number of elements in each window.
        /// </summary>
        [Description("The maximum number of elements in each window.")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip between the creation of
        /// consecutive windows.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the operator will generate consecutive
        /// non-overlapping windows.
        /// </remarks>
        [Description("The number of elements to skip between the creation of consecutive windows.")]
        public int? Skip { get; set; }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more windows
        /// based on element count information.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <returns>An observable sequence of windows.</returns>
        public override IObservable<IObservable<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            var skip = Skip;
            if (skip.HasValue) return source.Window(Count, skip.Value);
            else return source.Window(Count);
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="WindowCount"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(WindowCount))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ElementCountWindow : WindowCount
    {
    }
}
