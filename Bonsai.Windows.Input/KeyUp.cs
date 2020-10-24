using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    [DefaultProperty(nameof(Filter))]
    [Description("Produces a sequence of events whenever a keyboard key is released.")]
    public class KeyUp : Source<Keys>
    {
        [Description("The target keys to be observed.")]
        public Keys Filter { get; set; }

        public override IObservable<Keys> Generate()
        {
            return InterceptKeys.Instance.KeyUp.Where(InterceptKeys.GetKeyFilter(Filter));
        }
    }
}
