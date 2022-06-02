using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides the abstract base class for type visualizers which are designed to be
    /// combined with a <see cref="DialogMashupVisualizer"/>.
    /// </summary>
    [Obsolete]
    public abstract class MashupTypeVisualizer : DialogTypeVisualizer
    {
        /// <inheritdoc/>
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
