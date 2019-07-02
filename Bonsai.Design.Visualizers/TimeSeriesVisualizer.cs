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

        internal override RollingGraphView CreateView()
        {
            var view = base.CreateView();
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

        public override void Show(object value)
        {
            var array = value as Array;
            if (array != null)
            {
                XDate time = DateTime.Now;
                if (array.Length != view.Graph.NumSeries) view.Graph.NumSeries = array.Length;
                view.Graph.AddValues(time, array);
                UpdateView(DateTime.Now);
            }
            else base.Show(value);
        }
    }

    public class TimeSeriesVisualizerBase : DialogTypeVisualizer
    {
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30);

        readonly int numSeries;
        internal RollingGraphView view;
        DateTimeOffset updateTime;

        public TimeSeriesVisualizerBase()
            : this(1)
        {
        }

        public TimeSeriesVisualizerBase(int numSeries)
        {
            this.numSeries = numSeries;
        }

        protected GraphControl Graph
        {
            get { return view.Graph; }
        }

        internal virtual RollingGraphView CreateView()
        {
            var view = new RollingGraphView();
            view.Graph.GraphPane.XAxis.Type = AxisType.DateAsOrdinal;
            view.Graph.GraphPane.XAxis.Title.Text = "Time";
            view.Graph.GraphPane.XAxis.Title.IsVisible = true;
            view.Graph.GraphPane.XAxis.Scale.Format = "HH:mm:ss";
            view.Graph.GraphPane.XAxis.Scale.MajorUnit = DateUnit.Second;
            view.Graph.GraphPane.XAxis.Scale.MinorUnit = DateUnit.Millisecond;
            view.Graph.GraphPane.XAxis.MinorTic.IsAllTics = false;
            view.NumSeries = numSeries;
            return view;
        }

        internal void UpdateView(DateTime time)
        {
            if ((time - updateTime) > TargetElapsedTime)
            {
                Graph.Invalidate();
                updateTime = time;
            }
        }

        protected void AddValue(DateTime time, params object[] value)
        {
            view.AddValues((XDate)time, value);
            UpdateView(time);
        }

        public override void Show(object value)
        {
            AddValue(DateTime.Now, value);
        }

        public override void Load(IServiceProvider provider)
        {
            view = CreateView();
            view.Dock = DockStyle.Fill;

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(view);
            }
        }

        public override void Unload()
        {
            view.Dispose();
            view = null;
        }
    }
}
