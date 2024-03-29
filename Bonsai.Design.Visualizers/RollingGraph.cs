﻿using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    abstract class RollingGraph : GraphControl
    {
        int capacity;
        int numSeries;
        bool autoScaleX;
        bool autoScaleY;
        IPointListEdit[] series;
        RollingPointPairList[] rollingSeries;
        const int DefaultCapacity = 640;
        const int DefaultNumSeries = 1;

        public RollingGraph()
        {
            autoScaleX = true;
            autoScaleY = true;
            IsShowContextMenu = false;
            capacity = DefaultCapacity;
            numSeries = DefaultNumSeries;
            ZoomEvent += RollingGraph_ZoomEvent;
        }

        protected IPointListEdit[] Series
        {
            get { return series; }
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

        public double XMin
        {
            get { return GraphPane.XAxis.Scale.Min; }
            set
            {
                GraphPane.XAxis.Scale.Min = value;
                GraphPane.AxisChange();
                Invalidate();
            }
        }

        public double XMax
        {
            get { return GraphPane.XAxis.Scale.Max; }
            set
            {
                GraphPane.XAxis.Scale.Max = value;
                GraphPane.AxisChange();
                Invalidate();
            }
        }

        public double YMin
        {
            get { return GraphPane.YAxis.Scale.Min; }
            set
            {
                GraphPane.YAxis.Scale.Min = value;
                GraphPane.AxisChange();
                Invalidate();
            }
        }

        public double YMax
        {
            get { return GraphPane.YAxis.Scale.Max; }
            set
            {
                GraphPane.YAxis.Scale.Max = value;
                GraphPane.AxisChange();
                Invalidate();
            }
        }

        public bool AutoScaleX
        {
            get { return autoScaleX; }
            set
            {
                var changed = autoScaleX != value;
                autoScaleX = value;
                if (changed)
                {
                    GraphPane.XAxis.Scale.MaxAuto = autoScaleX;
                    GraphPane.XAxis.Scale.MinAuto = autoScaleX;
                    if (autoScaleX) Invalidate();
                }
            }
        }

        public bool AutoScaleY
        {
            get { return autoScaleY; }
            set
            {
                var changed = autoScaleY != value;
                autoScaleY = value;
                if (changed)
                {
                    GraphPane.YAxis.Scale.MaxAuto = autoScaleY;
                    GraphPane.YAxis.Scale.MinAuto = autoScaleY;
                    if (autoScaleY) Invalidate();
                }
            }
        }

        internal abstract CurveItem CreateSeries(string label, IPointListEdit points, Color color);

        private void EnsureSeries(string[] labels)
        {
            var hasLabels = labels != null;
            if (GraphPane.CurveList.Count != series.Length)
            {
                ResetColorCycle();
                GraphPane.CurveList.Clear();
                for (int i = 0; i < series.Length; i++)
                {
                    var curve = CreateSeries(
                        label: hasLabels ? labels[i] : string.Empty,
                        points: series[i],
                        color: GetNextColor());
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

            var previousSeries = series;
            if (capacity == 0 && rollingSeries != null)
            {
                rollingSeries = null;
                series = new IPointListEdit[numSeries];
            }

            for (int i = 0; i < series.Length; i++)
            {
                var previousPoints = previousSeries[i];
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

        public void AddValues(params PointPair[] values)
        {
            for (int i = 0; i < series.Length; i++)
            {
                series[i].Add(values[i]);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.P)
            {
                DoPrint();
            }

            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.S)
            {
                SaveAs();
            }

            if (e.KeyCode == Keys.Back)
            {
                ZoomOut(GraphPane);
            }

            base.OnKeyDown(e);
        }

        private void RollingGraph_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            MasterPane.AxisChange();
        }
    }
}
