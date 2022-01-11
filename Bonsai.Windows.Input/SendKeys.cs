using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Windows.Input
{
    /// <summary>
    /// Represents an operator that sends one or more keystrokes to the active window as
    /// if typed at the keyboard.
    /// </summary>
    [DefaultProperty(nameof(Keys))]
    [Description("Sends one or more keystrokes to the active window as if typed at the keyboard.")]
    public class SendKeys : Sink
    {
        /// <summary>
        /// Gets or sets the string expression specifying which keystrokes to send.
        /// For more details on expression format, see the remarks section of the
        /// <see cref="System.Windows.Forms.SendKeys"/> class.
        /// </summary>
        [Description("The string expression specifying which keystrokes to send.")]
        public string Keys { get; set; }

        /// <summary>
        /// Sends one or more keystrokes to the active window, as if typed at the keyboard,
        /// when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications indicating when to send the keystrokes to
        /// the active window.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of sending the specified keystrokes to the
        /// active window whenever the sequence emits a notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Do(input => System.Windows.Forms.SendKeys.Send(Keys));
        }
    }
}
