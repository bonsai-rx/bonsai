using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer<int, int>), Target = typeof(Tuple<int, int>))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer<float, float>), Target = typeof(Tuple<float, float>))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer<double, double>), Target = typeof(Tuple<double, double>))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer<int, int, int>), Target = typeof(Tuple<int, int, int>))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer<float, float, float>), Target = typeof(Tuple<float, float, float>))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer<double, double, double>), Target = typeof(Tuple<double, double, double>))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer<int, int, int, int>), Target = typeof(Tuple<int, int, int, int>))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer<float, float, float, float>), Target = typeof(Tuple<float, float, float, float>))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer<double, double, double, double>), Target = typeof(Tuple<double, double, double, double>))]

namespace Bonsai.Design.Visualizers
{
    public class TimeSeriesVisualizer : TimeSeriesVisualizerBase
    {
        public TimeSeriesVisualizer()
            : this(1)
        {
        }

        public TimeSeriesVisualizer(int numSeries)
            : base(numSeries)
        {
            AutoScale = true;
            Capacity = 640;
            Max = 1;
        }

        public int Capacity { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public bool AutoScale { get; set; }

        internal override TimeSeriesView CreateView()
        {
            var graph = base.CreateView();
            graph.Capacity = Capacity;
            graph.AutoScale = AutoScale;
            if (!AutoScale)
            {
                graph.Min = Min;
                graph.Max = Max;
            }

            graph.HandleDestroyed += delegate
            {
                Min = graph.Min;
                Max = graph.Max;
                AutoScale = graph.AutoScale;
                Capacity = graph.Capacity;
            };
            return graph;
        }
    }

    public class TimeSeriesVisualizer<T1, T2> : TimeSeriesVisualizer
    {
        public TimeSeriesVisualizer()
            : base(2)
        {
        }

        public override void Show(object value)
        {
            var tuple = (Tuple<T1, T2>)value;
            AddValue(DateTime.Now, tuple.Item1, tuple.Item2);
        }
    }

    public class TimeSeriesVisualizer<T1, T2, T3> : TimeSeriesVisualizer
    {
        public TimeSeriesVisualizer()
            : base(3)
        {
        }

        public override void Show(object value)
        {
            var tuple = (Tuple<T1, T2, T3>)value;
            AddValue(DateTime.Now, tuple.Item1, tuple.Item2, tuple.Item3);
        }
    }

    public class TimeSeriesVisualizer<T1, T2, T3, T4> : TimeSeriesVisualizer
    {
        public TimeSeriesVisualizer()
            : base(4)
        {
        }

        public override void Show(object value)
        {
            var tuple = (Tuple<T1, T2, T3, T4>)value;
            AddValue(DateTime.Now, tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
        }
    }

    public class TimeSeriesVisualizerBase : DialogTypeVisualizer
    {
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30);

        readonly int numSeries;
        TimeSeriesView graph;
        DateTimeOffset updateTime;

        public TimeSeriesVisualizerBase()
            : this(1)
        {
        }

        public TimeSeriesVisualizerBase(int numSeries)
        {
            this.numSeries = numSeries;
        }

        protected ChartControl Chart
        {
            get { return graph.Chart; }
        }

        internal virtual TimeSeriesView CreateView()
        {
            var graph = new TimeSeriesView();
            graph.NumSeries = numSeries;
            return graph;
        }

        protected void AddValue(DateTime time, params object[] value)
        {
            graph.AddValues(time, value);

            if ((time - updateTime) > TargetElapsedTime)
            {
                Chart.Invalidate();
                updateTime = time;
            }
        }

        public override void Show(object value)
        {
            AddValue(DateTime.Now, value);
        }

        public override void Load(IServiceProvider provider)
        {
            graph = CreateView();
            graph.Dock = DockStyle.Fill;

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(graph);
            }
        }

        public override void Unload()
        {
            graph.Dispose();
            graph = null;
        }
    }
}
