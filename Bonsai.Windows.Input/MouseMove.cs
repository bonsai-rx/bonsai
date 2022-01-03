using System;
using System.ComponentModel;
using System.Drawing;

namespace Bonsai.Windows.Input
{
    /// <summary>
    /// Represents an operator that generates a sequence of cursor positions whenever the mouse moves.
    /// </summary>
    [Description("Generates a sequence of cursor positions whenever the mouse moves.")]
    public class MouseMove : Source<Point>
    {
        /// <summary>
        /// Generates an observable sequence of cursor positions whenever the mouse moves.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Point"/> values representing the current
        /// cursor position whenever the mouse moves.
        /// </returns>
        public override IObservable<Point> Generate()
        {
            return InterceptMouse.Instance.MouseMove;
        }
    }
}
