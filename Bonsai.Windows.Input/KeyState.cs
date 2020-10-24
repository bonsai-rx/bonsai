using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Windows.Input
{
    [DefaultProperty(nameof(Filter))]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Determines whether a key is up or down at the time of notification.")]
    public class KeyState : Combinator<bool>
    {
        [Description("The target key to be observed.")]
        public Keys Filter { get; set; }

        public override IObservable<bool> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => InterceptKeys.GetKeyState(Filter));
        }
    }
}
