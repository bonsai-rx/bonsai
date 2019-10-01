using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    class RollingGraph : GraphControl
    {
        int capacity;
        int numSeries;
        bool autoScale;
        float lineWidth;
        SymbolType symbolType;
        RollingPointPairList[] series;
        const int DefaultCapacity = 640;
        const int DefaultNumSeries = 1;

        public RollingGraph()
        {
            autoScale = true;
            IsShowContextMenu = false;
            capacity = DefaultCapacity;
            numSeries = DefaultNumSeries;
            symbolType = SymbolType.None;
            lineWidth = 1;
        }

        public SymbolType SymbolType
        {
            get { return symbolType; }
            set { symbolType = value; }
        }

        public float LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; }
        }

        public int NumSeries
        {
            get { return numSeries; }
            set
            {
                numSeries = value;
                if (series != null)
                {
                    series = null;
                    EnsureCapacity();
                }
            }
        }

        public int Capacity
        {
            get { return capacity; }
            set
            {
                capacity = value;
                EnsureCapacity();
                Invalidate();
            }
        }

        public double Min
        {
            get { return GraphPane.YAxis.Scale.Min; }
            set
            {
                GraphPane.YAxis.Scale.Min = value;
                GraphPane.AxisChange();
                Invalidate();
            }
        }

        public double Max
        {
            get { return GraphPane.YAxis.Scale.Max; }
            set
            {
                GraphPane.YAxis.Scale.Max = value;
                GraphPane.AxisChange();
                Invalidate();
            }
        }

        public bool AutoScale
        {
            get { return autoScale; }
            set
            {
                var changed = autoScale != value;
                autoScale = value;
                if (changed)
                {
                    GraphPane.YAxis.Scale.MaxAuto = autoScale;
                    GraphPane.YAxis.Scale.MinAuto = autoScale;
                    if (autoScale) Invalidate();
                }
            }
        }

        private void EnsureSeries()
        {
            if (GraphPane.CurveList.Count != series.Length)
            {
                ResetColorCycle();
                GraphPane.CurveList.Clear();
                for (int i = 0; i < series.Length; i++)
                {
                    var curve = new LineItem(string.Empty, series[i], GetNextColor(), symbolType, lineWidth);
                    curve.Line.IsAntiAlias = true;
                    curve.Line.IsOptimizedDraw = true;
                    curve.Label.IsVisible = false;
                    GraphPane.CurveList.Add(curve);
                }
            }
        }

        public void EnsureCapacity()
        {
            series = series ?? new RollingPointPairList[numSeries];
            for (int i = 0; i < series.Length; i++)
            {
                var points = new RollingPointPairList(capacity);
                var previousPoints = series[i];
                if (previousPoints != null)
                {
                    points.Add(previousPoints);
                }

                series[i] = points;
            }

            EnsureSeries();
            for (int i = 0; i < series.Length; i++)
            {
                GraphPane.CurveList[i].Points = series[i];
            }
        }

        public void AddValues(double index, params double[] values)
        {
            for (int i = 0; i < series.Length; i++)
            {
                series[i].Add(index, values[i]);
            }
        }

        public void AddValues(double index, params object[] values)
        {
            for (int i = 0; i < series.Length; i++)
            {
                series[i].Add(index, Convert.ToDouble(values[i]));
            }
        }

        public void AddValues(double index, Array values)
        {
            for (int i = 0; i < series.Length; i++)
            {
                series[i].Add(index, Convert.ToDouble(values.GetValue(i)));
            }
        }
    }
}
