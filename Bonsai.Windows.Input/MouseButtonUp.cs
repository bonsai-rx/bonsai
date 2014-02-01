using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    public class MouseButtonUp : Source<MouseButtons>
    {
        public override IObservable<MouseButtons> Generate()
        {
            return InterceptMouse.Instance.MouseButtonUp;
        }
    }
}
