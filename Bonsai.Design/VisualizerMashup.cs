using Bonsai.Expressions;
using System;

namespace Bonsai.Design
{
    public class VisualizerMashup : ITypeVisualizerContext
    {
        public VisualizerMashup(InspectBuilder source, MashupTypeVisualizer visualizer)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
        }
        public InspectBuilder Source { get; private set; }

        public MashupTypeVisualizer Visualizer { get; private set; }
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
