using System;
using System.Xml.Serialization;

namespace Bonsai.Design.Visualizers
{
    public abstract class MashupVisualizer : MashupTypeVisualizer
    {
        public abstract Type VisualizerType { get; }
    }

    [XmlRoot("MashupSettings")]
    public class MashupVisualizer<TVisualizer> : MashupVisualizer where TVisualizer : DialogTypeVisualizer, new()
    {
        [XmlElement("VisualizerSettings")]
        public TVisualizer Visualizer { get; set; }

        public override Type VisualizerType => typeof(TVisualizer);

        public override void Load(IServiceProvider provider)
        {
            Visualizer ??= new TVisualizer();
            Visualizer.Load(provider);
        }

        public override void Show(object value)
        {
            Visualizer.Show(value);
        }

        public override void Unload()
        {
            Visualizer.Unload();
        }

        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            return Visualizer.Visualize(source, provider);
        }

        public override void SequenceCompleted()
        {
            Visualizer.SequenceCompleted();
        }
    }
}
