using System;
using System.ComponentModel;
using System.Drawing;

namespace Bonsai.Windows.Input
{
    [Description("Produces a sequence of cursor coordinates whenever the mouse moves.")]
    public class MouseMove : Source<Point>
    {
        public override IObservable<Point> Generate()
        {
            return InterceptMouse.Instance.MouseMove;
        }
    }
}
