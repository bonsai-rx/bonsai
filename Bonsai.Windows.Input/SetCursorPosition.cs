using System;
using System.ComponentModel;
using System.Drawing;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    [Description("Sets the current position of the mouse cursor.")]
    public class SetCursorPosition : Sink<Point>
    {
        public override IObservable<Point> Process(IObservable<Point> source)
        {
            return source.Do(input => Cursor.Position = input);
        }
    }
}
