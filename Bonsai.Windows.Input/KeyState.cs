using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Determines whether a key is up or down at the time of notification.")]
    public class KeyState : Combinator<bool>
    {
        [Description("The target key to be observed.")]
        public Keys Filter { get; set; }

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys key);

        public override IObservable<bool> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => (GetAsyncKeyState(Filter) & 0x8000) != 0);
        }
    }
}
