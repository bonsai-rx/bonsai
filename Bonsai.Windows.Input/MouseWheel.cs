using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Windows.Input
{
    [Description("Produces a sequence of values whenever the mouse wheel moves.")]
    public class MouseWheel : Source<int>
    {
        public override IObservable<int> Generate()
        {
            return InterceptMouse.Instance.MouseWheel;
        }
    }
}
