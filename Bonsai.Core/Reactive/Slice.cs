using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that extracts a range of elements from an observable sequence.
    /// </summary>
    [DefaultProperty(nameof(Step))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Extracts a range of elements from an observable sequence.")]
    public class Slice : Combinator
    {
        /// <summary>
        /// Gets or sets the element index at which the slice begins.
        /// </summary>
        [Description("The element index at which the slice begins.")]
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the number of elements to skip between slice elements.
        /// </summary>
        [Description("The number of elements to skip between slice elements.")]
        public int Step { get; set; } = 1;

        /// <summary>
        /// Gets or sets the element index at which the slice ends. If no value
        /// is specified, elements will be taken until the end of the sequence.
        /// </summary>
        [Description("The element index at which the slice ends. If no value is specified, elements will be taken until the end of the sequence.")]
        public int? Stop { get; set; }

        /// <summary>
        /// Extracts a range of elements from an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to slice.</param>
        /// <returns>The sliced sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var start = Start;
            if (start < 0)
            {
                throw new InvalidOperationException("The specified start index is less than zero.");
            }

            var step = Step;
            if (step <= 0)
            {
                throw new InvalidOperationException("Step size must be a positive number.");
            }

            var stop = Stop;
            if (stop < 0)
            {
                throw new InvalidOperationException("The specified stop index is less than zero.");
            }

            return (stop - start) <= 0 ? Observable.Empty<TSource>() : Observable.Create<TSource>(observer =>
            {
                int i = 0;
                var sliceObserver = Observer.Create<TSource>(
                    value =>
                    {
                        var index = i++;
                        if (index >= start && (index - start) % step == 0)
                        {
                            observer.OnNext(value);
                        }

                        if (i >= stop)
                        {
                            observer.OnCompleted();
                        }
                    },
                    observer.OnError,
                    observer.OnCompleted);
                return source.SubscribeSafe(sliceObserver);
            });
        }
    }
}
