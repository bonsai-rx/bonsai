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

        internal override RollingGraphView CreateView()
        {
            var view = new BooleanTimeSeriesView();
            view.Capacity = Capacity;
            view.HandleDestroyed += delegate
            {
                Capacity = view.Capacity;
            };
            return view;
        }
    }
}
