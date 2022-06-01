using System;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a wrapper to convert any <see cref="DialogTypeVisualizer"/> into a
    /// <see cref="MashupTypeVisualizer"/>.
    /// </summary>
    public class MashupVisualizerAdapter : MashupTypeVisualizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MashupVisualizerAdapter"/> class.
        /// </summary>
        /// <param name="visualizer">The visualizer to be wrapped in the mashup.</param>
        public MashupVisualizerAdapter(DialogTypeVisualizer visualizer)
        {
            Visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
        }

        /// <summary>
        /// Gets the underlying visualizer object.
        /// </summary>
        public DialogTypeVisualizer Visualizer { get; set; }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            Visualizer.Load(provider);
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            Visualizer.Show(value);
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            Visualizer.Unload();
        }

        /// <inheritdoc/>
        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            return Visualizer.Visualize(source, provider);
        }

        /// <inheritdoc/>
        public override void SequenceCompleted()
        {
            Visualizer.SequenceCompleted();
        }
    }
}
