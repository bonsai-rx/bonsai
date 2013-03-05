using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Design
{
    public class VisualizerMashup
    {
        public VisualizerMashup(IObservable<object> source, DialogTypeVisualizer visualizer)
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

        public IObservable<object> Source { get; private set; }

        public DialogTypeVisualizer Visualizer { get; private set; }
    }

    public static class VisualizerMashup<TMashupVisualizer, TVisualizer>
        where TMashupVisualizer : DialogMashupVisualizer
        where TVisualizer : DialogTypeVisualizer
    {
    }
}
