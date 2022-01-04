using Bonsai;
using Bonsai.Design.Visualizers;

[assembly: TypeVisualizer(typeof(BooleanTimeSeriesVisualizer), Target = typeof(bool))]

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer for boolean time series data.
    /// </summary>
    public class BooleanTimeSeriesVisualizer : TimeSeriesVisualizerBase
    {
        /// <summary>
        /// Gets or sets the maximum number of time points displayed at any one moment in the graph.
        /// </summary>
        public int Capacity { get; set; } = 640;

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
