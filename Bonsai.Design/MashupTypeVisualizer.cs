using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;

namespace Bonsai.Design
{
    public abstract class MashupTypeVisualizer : DialogTypeVisualizer
    {
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            var visualizerDialog = (TypeVisualizerDialog)provider.GetService(typeof(TypeVisualizerDialog));
            if (visualizerDialog != null)
            {
                return source.SelectMany(xs => xs.Do(ys => { }, () => visualizerDialog.BeginInvoke((Action)SequenceCompleted)));
            }

            return source;
        }
    }
}
