using Bonsai.Expressions;
using System;

namespace Bonsai.Design
{
    public class VisualizerMashup : ITypeVisualizerContext
    {
        readonly InspectBuilder inspectBuilder;

        public VisualizerMashup(InspectBuilder source, MashupTypeVisualizer visualizer)
        {
            inspectBuilder = source ?? throw new ArgumentNullException(nameof(source));
            Visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
            Source = inspectBuilder.Output;
        }

        [Obsolete]
        public VisualizerMashup(IObservable<IObservable<object>> source, MashupTypeVisualizer visualizer)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
        }

        [Obsolete]
        public IObservable<IObservable<object>> Source { get; private set; }

        public MashupTypeVisualizer Visualizer { get; private set; }

        InspectBuilder ITypeVisualizerContext.Source => inspectBuilder;
    }

    public static class VisualizerMashup<TMashupVisualizer>
        where TMashupVisualizer : DialogMashupVisualizer
    {
    }

    public static class VisualizerMashup<TMashupVisualizer, TVisualizer>
        where TMashupVisualizer : DialogMashupVisualizer
        where TVisualizer : DialogTypeVisualizer
    {
    }
}
