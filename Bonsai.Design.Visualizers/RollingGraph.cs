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
        IPointListEdit[] series;
        RollingPointPairList[] rollingSeries;
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
        }

        public int Capacity
        {
            get { return capacity; }
            set
            {
                capacity = value;
                EnsureCapacity(numSeries);
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

        private void EnsureSeries(string[] labels)
        {
            var hasLabels = labels != null;
            if (GraphPane.CurveList.Count != series.Length)
            {
                ResetColorCycle();
                GraphPane.CurveList.Clear();
                for (int i = 0; i < series.Length; i++)
                {
                    var curve = new LineItem(hasLabels ? labels[i] : string.Empty, series[i], GetNextColor(), symbolType, lineWidth);
                    curve.Line.IsAntiAlias = true;
                    curve.Line.IsOptimizedDraw = true;
                    curve.Label.IsVisible = hasLabels;
                    GraphPane.CurveList.Add(curve);
                }
            }
            else
            {
                for (int i = 0; i < series.Length; i++)
                {
                    GraphPane.CurveList[i].Points = series[i];
                }
            }
        }

        public void EnsureCapacity(int count, string[] labels = null, bool reset = false)
        {
            numSeries = count;
            if (series == null || series.Length != numSeries || reset)
            {
                if (capacity == 0)
                {
                    rollingSeries = null;
                    series = new IPointListEdit[numSeries];
                }
                else
                {
                    rollingSeries = new RollingPointPairList[numSeries];
                    series = rollingSeries;
                }
            }

            for (int i = 0; i < series.Length; i++)
            {
                var previousPoints = series[i];
                if (capacity > 0)
                {
                    var points = new RollingPointPairList(capacity);
                    if (previousPoints != null)
                    {
                        points.Add(previousPoints);
                    }

                    series[i] = points;
                }
                else
                {
                    var points = new PointPairList();
                    if (previousPoints != null)
                    {
                        for (int p = 0; p < previousPoints.Count; p++)
                        {
                            points.Add(previousPoints[p]);
                        }
                    }

                    series[i] = points;
                }
            }

            EnsureSeries(labels);
        }

        public void AddValues(double index, params double[] values) => AddValues(index, null, values);

        public void AddValues(double index, object tag, params double[] values)
        {
            if (rollingSeries != null)
            {
                for (int i = 0; i < rollingSeries.Length; i++)
                {
                    rollingSeries[i].Add(index, values[i], tag);
                }
            }
            else
            {
                for (int i = 0; i < series.Length; i++)
                {
                    series[i].Add(new PointPair(index, values[i], double.MaxValue, tag));
                }
            }
        }
    }
}
