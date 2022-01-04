using System;
using System.Xml.Serialization;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides the non-generic abstract base class used to wrap any
    /// <see cref="DialogTypeVisualizer"/> into a <see cref="MashupTypeVisualizer"/>.
    /// </summary>
    public abstract class MashupVisualizer : MashupTypeVisualizer
    {
        /// <summary>
        /// Gets the type of the underlying visualizer object.
        /// </summary>
        public abstract Type VisualizerType { get; }
    }

    /// <summary>
    /// Provides a wrapper to convert any <see cref="DialogTypeVisualizer"/> into a
    /// <see cref="MashupTypeVisualizer"/>.
    /// </summary>
    /// <typeparam name="TVisualizer">The type of the visualizer to be wrapped in the mashup.</typeparam>
    [XmlRoot("MashupSettings")]
    public class MashupVisualizer<TVisualizer> : MashupVisualizer where TVisualizer : DialogTypeVisualizer, new()
    {
        /// <summary>
        /// Gets the underlying visualizer object.
        /// </summary>
        [XmlElement("VisualizerSettings")]
        public TVisualizer Visualizer { get; set; }

        /// <summary>
        /// Gets the type of the underlying visualizer object.
        /// </summary>
        public override Type VisualizerType => typeof(TVisualizer);

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            Visualizer ??= new TVisualizer();
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
