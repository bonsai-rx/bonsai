using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
