using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    [Description("Produces a sequence of events whenever a keyboard key is depressed.")]
    public class KeyDown : Source<Keys>
    {
        [Description("The target keys to be observed.")]
        public Keys Filter { get; set; }

        public override IObservable<Keys> Generate()
        {
            return InterceptKeys.Instance.KeyDown
                .Where(key => Filter == Keys.None || key == Filter);
        }
    }
}
