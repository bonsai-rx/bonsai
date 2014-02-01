using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    public class KeyUp : Source<Keys>
    {
        public override IObservable<Keys> Generate()
        {
            return InterceptKeys.Instance.KeyUp;
        }
    }
}
