using System;
using System.ComponentModel;

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
