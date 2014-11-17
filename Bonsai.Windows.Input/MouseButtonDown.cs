using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    public class MouseButtonDown : Source<MouseButtons>
    {
        public MouseButtons Filter { get; set; }

        public override IObservable<MouseButtons> Generate()
        {
            return InterceptMouse.Instance.MouseButtonDown
                .Where(button => Filter == MouseButtons.None || button == Filter);
        }
    }
}
