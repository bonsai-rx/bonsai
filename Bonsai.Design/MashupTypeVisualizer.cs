using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public abstract class MashupTypeVisualizer : DialogTypeVisualizer
    {
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            var visualizerControl = provider.GetService(typeof(IDialogTypeVisualizerService)) as Control;
            if (visualizerControl != null)
            {
                return source.SelectMany(xs => xs.Do(ys => { }, () => visualizerControl.BeginInvoke((Action)SequenceCompleted)));
            }

            return source;
        }
    }
}
