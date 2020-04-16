using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    [Description("Produces a sequence of events whenever a mouse button is depressed.")]
    public class MouseButtonDown : Source<MouseButtons>
    {
        [Description("The target buttons to be observed.")]
        public MouseButtons Filter { get; set; }

        public override IObservable<MouseButtons> Generate()
        {
            return InterceptMouse.Instance.MouseButtonDown
                .Where(button => Filter == MouseButtons.None || button == Filter);
        }
    }
}
