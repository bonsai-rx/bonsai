using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Design
{
    public class VisualizerMashup
    {
        public VisualizerMashup(IObservable<IObservable<object>> source, MashupTypeVisualizer visualizer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (visualizer == null)
            {
                throw new ArgumentNullException("visualizer");
            }

            Source = source;
            Visualizer = visualizer;
        }

        public IObservable<IObservable<object>> Source { get; private set; }

        public MashupTypeVisualizer Visualizer { get; private set; }
    }

    public static class VisualizerMashup<TMashupVisualizer, TVisualizer>
        where TMashupVisualizer : DialogMashupVisualizer
        where TVisualizer : DialogTypeVisualizer
    {
    }
}
