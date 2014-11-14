using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    public class KeyDown : Source<Keys>
    {
        public Keys Filter { get; set; }

        public override IObservable<Keys> Generate()
        {
            return InterceptKeys.Instance.KeyDown
                .Where(key => Filter == Keys.None || key == Filter);
        }
    }
}
