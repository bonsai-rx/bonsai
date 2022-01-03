using System;
using System.ComponentModel;
using System.Drawing;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    /// <summary>
    /// Represents an operator that sets the current position of the mouse cursor.
    /// </summary>
    [Description("Sets the current position of the mouse cursor.")]
    public class SetCursorPosition : Sink<Point>
    {
        /// <summary>
        /// Sets the current position of the mouse cursor using an observable sequence
        /// of point values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Point"/> values representing the position to set the cursor to.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of updating the cursor position.
        /// </returns>
        public override IObservable<Point> Process(IObservable<Point> source)
        {
            return source.Do(input => Cursor.Position = input);
        }
    }
}
