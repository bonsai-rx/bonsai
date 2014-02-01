using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Windows.Input
{
    public class MouseWheel : Source<int>
    {
        public override IObservable<int> Generate()
        {
            return InterceptMouse.Instance.MouseWheel;
        }
    }
}
