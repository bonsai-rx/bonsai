using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    [Description("Produces a sequence of events whenever a mouse button is released.")]
    public class MouseButtonUp : Source<MouseButtons>
    {
        [Description("The target buttons to be observed.")]
        public MouseButtons Filter { get; set; }

        public override IObservable<MouseButtons> Generate()
        {
            return InterceptMouse.Instance.MouseButtonUp
                .Where(button => Filter == MouseButtons.None || button == Filter);
        }
    }
}
