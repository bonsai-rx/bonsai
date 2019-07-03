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
        {
            NumSeries = numSeries;
            AutoScale = true;
            Capacity = 640;
            Max = 1;
        }

        private int NumSeries { get; set; }

        public int Capacity { get; set; }

        public double Min { get; set; }

        public double Max { get; set; }

        public bool AutoScale { get; set; }

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
            GraphHelper.FormatDateAxis(view.Graph.GraphPane.XAxis);
            GraphHelper.SetAxisLabel(view.Graph.GraphPane.XAxis, "Time");

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
