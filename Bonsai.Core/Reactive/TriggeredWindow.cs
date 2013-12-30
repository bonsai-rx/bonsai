using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that projects each element of an observable sequence into consecutive
    /// non-overlapping windows.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into non-overlapping windows. A window is closed when the second sequence produces an element.")]
    public class TriggeredWindow
    {
        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping windows.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TWindowClosing">
        /// The type of the elements in the sequence indicating window closing events.
        /// </typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <param name="windowClosing">
        /// The sequence of window closing events. The current window is closed and a new window
        /// is opened upon receiving a window closing event.
        /// </param>
        /// <returns>An observable sequence of windows.</returns>
        public IObservable<IObservable<TSource>> Process<TSource, TWindowClosing>(IObservable<TSource> source, IObservable<TWindowClosing> windowClosing)
        {
            return source.Window(() => windowClosing);
        }
    }
}
