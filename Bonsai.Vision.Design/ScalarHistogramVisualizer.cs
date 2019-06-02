using Bonsai;
using Bonsai.Design;
using Bonsai.Design.Visualizers;
using Bonsai.Vision;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZedGraph;

[assembly: TypeVisualizer(typeof(ScalarHistogramVisualizer), Target = typeof(ScalarHistogram))]

namespace Bonsai.Vision.Design
{
    public class ScalarHistogramVisualizer : DialogTypeVisualizer
    {
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30);

        CurveItem val0;
        CurveItem val1;
        CurveItem val2;
        CurveItem val3;
        GraphControl graph;
        DateTimeOffset updateTime;

        CurveItem CreateHistogramCurve(Color color)
        {
            var series = new LineItem(string.Empty, new PointPairList(), color, SymbolType.None);
            series.Line.IsAntiAlias = true;
            series.Line.IsOptimizedDraw = true;
            series.Label.IsVisible = false;
            graph.GraphPane.CurveList.Add(series);
            return series;
        }

        void UpdateHistogramValues(Histogram histogram, CurveItem values)
        {
            values.Clear();
            for (int i = 0; i < 256; i++)
            {
                values.AddPoint(i, histogram.QueryValue(i));
            }
        }

        void UpdateCurveItem(Histogram histogram, Color color, ref CurveItem values)
        {
            if (histogram == null)
            {
                graph.GraphPane.CurveList.Remove(values);
                values = null;
            }
            else
            {
                if (values == null)
                {
                    values = CreateHistogramCurve(color);
                }

                UpdateHistogramValues(histogram, values);
            }
        }

        void UpdateValues(ScalarHistogram value)
        {
            if (value == null)
            {
                graph.GraphPane.CurveList.Clear();
                val0 = val1 = val2 = val3 = null;
                return;
            }

            UpdateCurveItem(value.Val0, Color.Blue, ref val0);
            UpdateCurveItem(value.Val1, Color.Green, ref val1);
            UpdateCurveItem(value.Val2, Color.Red, ref val2);
            UpdateCurveItem(value.Val3, Color.Magenta, ref val3);
        }

        public override void Show(object value)
        {
            var time = DateTimeOffset.Now;
            if ((time - updateTime) > TargetElapsedTime)
            {
                var histogram = (ScalarHistogram)value;
                UpdateValues(histogram);
                graph.Invalidate();
                updateTime = time;
            }
        }

        public override void Load(IServiceProvider provider)
        {
            graph = new GraphControl();
            graph.Dock = DockStyle.Fill;
            graph.GraphPane.XAxis.Type = AxisType.Linear;
            graph.GraphPane.XAxis.Title.Text = "Intensity";
            graph.GraphPane.XAxis.Title.IsVisible = true;
            graph.GraphPane.XAxis.MinorTic.IsAllTics = false;

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
            val0 = null;
            val1 = null;
            val2 = null;
            val3 = null;
        }
    }
}
