using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public abstract class DialogTypeVisualizer
    {
        public abstract void Show(object value);

        public abstract void Load(IServiceProvider provider);

        public abstract void Unload();

        public virtual IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            var visualizerControl = provider.GetService(typeof(IDialogTypeVisualizerService)) as Control;
            if (visualizerControl != null)
            {
                return source.SelectMany(xs => xs.ObserveOn(visualizerControl).Do(Show, SequenceCompleted));
            }

            return source;
        }

        public virtual void SequenceCompleted()
        {
        }
    }
}
