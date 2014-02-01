using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Windows.Input
{
    public class MouseMove : Source<Point>
    {
        public override IObservable<Point> Generate()
        {
            return InterceptMouse.Instance.MouseMove;
        }
    }
}
