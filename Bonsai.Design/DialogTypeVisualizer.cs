using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides the abstract base class for type visualizers.
    /// </summary>
    public abstract class DialogTypeVisualizer
    {
        /// <summary>
        /// Updates the type visualizer to display the specified value object.
        /// </summary>
        /// <param name="value">The value to visualize.</param>
        public abstract void Show(object value);

        /// <summary>
        /// Loads type visualizer resources using the specified service provider.
        /// </summary>
        /// <param name="provider">
        /// A service provider object which can be used to obtain visualization,
        /// runtime inspection, or other editing services.
        /// </param>
        public abstract void Load(IServiceProvider provider);

        /// <summary>
        /// Unloads all type visualizer resources.
        /// </summary>
        public abstract void Unload();

        /// <summary>
        /// Creates an observable sequence used to visualize all notifications emitted by
        /// a workflow operator, using this type visualizer and the specified service provider.
        /// </summary>
        /// <param name="source">
        /// An observable sequence that multicasts notifications from all the active
        /// subscriptions to the workflow operator.
        /// </param>
        /// <param name="provider">
        /// A service provider object which can be used to obtain visualization,
        /// runtime inspection, or other editing services.
        /// </param>
        /// <returns>
        /// An observable sequence where every notification updates the type visualizer
        /// object in the UI thread.
        /// </returns>
        public virtual IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            var visualizerControl = provider.GetService(typeof(IDialogTypeVisualizerService)) as Control;
            if (visualizerControl != null)
            {
                return source.SelectMany(xs => xs.ObserveOn(visualizerControl).Do(Show, SequenceCompleted));
            }

            return source;
        }

        /// <summary>
        /// Updates the type visualizer when one of the active subscriptions gracefully terminates.
        /// </summary>
        public virtual void SequenceCompleted()
        {
        }
    }
}
