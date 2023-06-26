using System;
using Bonsai;
using Bonsai.Design.Visualizers;
using ZedGraph;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(byte))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(int))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(uint))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(long))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(ulong))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(short))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(ushort))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(float))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(double))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(byte[]))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(int[]))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(uint[]))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(long[]))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(ulong[]))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(short[]))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(ushort[]))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(float[]))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(double[]))]

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer for multi-dimensional time series data.
    /// </summary>
    public class TimeSeriesVisualizer : TimeSeriesVisualizerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSeriesVisualizer"/> class.
        /// </summary>
        public TimeSeriesVisualizer()
            : this(1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSeriesVisualizer"/> class
        /// using the specified number of dimensions.
        /// </summary>
        /// <param name="numSeries">
        /// The number of dimensions in the time series graph. Each dimension will be
        /// plotted on its own visual trace.
        /// </param>
        public TimeSeriesVisualizer(int numSeries)
        {
            NumSeries = numSeries;
            AutoScale = true;
            Capacity = 640;
            Max = 1;
        }

        private int NumSeries { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of time points displayed at any one moment in the graph.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the lower limit of the y-axis range when using a fixed scale.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// Gets or sets the upper limit of the y-axis range when using a fixed scale.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the y-axis range should be recalculated
        /// automatically as the graph updates.
        /// </summary>
        public bool AutoScale { get; set; }

        /// <summary>
        /// Gets the underlying graph control.
        /// </summary>
        [Obsolete]
        protected GraphControl Graph
        {
            get { return view.Graph; }
        }

        internal override RollingGraphView CreateView()
        {
            var view = base.CreateView();
            view.NumSeries = NumSeries;
            view.Capacity = Capacity;
            view.AutoScale = AutoScale;
            if (!AutoScale)
            {
                view.Min = Min;
                view.Max = Max;
            }

            view.HandleDestroyed += delegate
            {
                Min = view.Min;
                Max = view.Max;
                AutoScale = view.AutoScale;
                Capacity = view.Capacity;
            };
            return view;
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var array = value as Array;
            if (array != null)
            {
                XDate time = DateTime.Now;
                AddValue(time, array);
            }
            else base.Show(value);
        }
    }

    /// <summary>
    /// Provides a base class for rolling graph visualizers of multi-dimensional
    /// time series data.
    /// </summary>
    public class TimeSeriesVisualizerBase : BufferedVisualizer
    {
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30);
        internal RollingGraphView view;
        DateTimeOffset updateTime;

        internal virtual RollingGraphView CreateView()
        {
            return new RollingGraphView();
        }

        internal void UpdateView(DateTime time)
        {
            if ((time - updateTime) > TargetElapsedTime)
            {
                view.Graph.Invalidate();
                updateTime = time;
            }
        }

        /// <summary>
        /// Adds a new data point to the multi-dimensional time series.
        /// </summary>
        /// <param name="time">The timestamp associated with the data point.</param>
        /// <param name="value">
        /// An array representing all the attribute dimensions of the data point.
        /// </param>
        [Obsolete]
        protected void AddValue(DateTime time, params object[] value)
        {
            AddValue(time, Array.ConvertAll(value, x => Convert.ToDouble(x)));
        }

        /// <summary>
        /// Adds a new data point to the multi-dimensional time series.
        /// </summary>
        /// <param name="time">The timestamp associated with the data point.</param>
        /// <param name="value">
        /// A <see cref="double"/> array representing all the attribute dimensions
        /// of the data point.
        /// </param>
        protected void AddValue(DateTime time, params double[] value)
        {
            view.AddValues((XDate)time, value);
            UpdateView(time);
        }

        internal void AddValue(DateTime time, Array array)
        {
            if (array.Length != view.Graph.NumSeries)
            {
                view.Graph.EnsureCapacity(array.Length);
            }

            var values = new double[array.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Convert.ToDouble(array.GetValue(i));
            }

            AddValue(time, values);
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            AddValue(DateTime.Now, Convert.ToDouble(value));
        }

        /// <inheritdoc/>
        protected override void Show(DateTime time, object value)
        {
            if (value is IConvertible convertible)
            {
                AddValue(time, convertible.ToDouble(null));
            }
            else Show(value);
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            view = CreateView();
            view.Dock = DockStyle.Fill;
            GraphHelper.FormatDateAxis(view.Graph.GraphPane.XAxis);
            GraphHelper.SetAxisLabel(view.Graph.GraphPane.XAxis, "Time");

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(view);
            }
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            view.Dispose();
            view = null;
        }
    }
}
