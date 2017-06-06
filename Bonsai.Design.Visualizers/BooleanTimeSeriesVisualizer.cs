using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Design.Visualizers;
using ZedGraph;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(BooleanTimeSeriesVisualizer), Target = typeof(bool))]

namespace Bonsai.Design.Visualizers
{
    public class BooleanTimeSeriesVisualizer : TimeSeriesVisualizerBase
    {
        public BooleanTimeSeriesVisualizer()
        {
            Capacity = 640;
        }

        public int Capacity { get; set; }

        internal override TimeSeriesView CreateView()
        {
            var graph = new BooleanTimeSeriesView();
            graph.Capacity = Capacity;
            graph.HandleDestroyed += delegate
            {
                Capacity = graph.Capacity;
            };
            return graph;
        }
    }
}
