using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bonsai.Design.Visualizers;
using ZedGraph;
using System.Globalization;
using Bonsai.Dsp.Design.Properties;

namespace Bonsai.Dsp.Design
{
    public partial class WaveformGraph : ChartControl
    {
        const float TitleFontSize = 10;
        const float YAxisMinSpace = 50;
        const int MaxSamplePoints = 1000;
        const float DefaultPaneMargin = 10;
        const float DefaultPaneTitleGap = 0.5f;
        const float TilePaneHorizontalMargin = 5;

        bool autoScaleX;
        bool autoScaleY;
        int sequenceIndex;
        bool overlayChannels;
        int historyLength;
        double channelOffset;
        int waveformBufferLength;
        DownsampledPointPairList[] values;

        public WaveformGraph()
        {
            overlayChannels = true;
            WaveformBufferLength = 1;
            IsShowContextMenu = false;
            MasterPane.InnerPaneGap = 0;
            GraphPane.IsFontsScaled = false;
            GraphPane.Title.FontSpec.IsBold = false;
            GraphPane.Title.FontSpec.Size = TitleFontSize;
            GraphPane.Title.Text = (0).ToString(CultureInfo.InvariantCulture);
            GraphPane.XAxis.Type = AxisType.Linear;
            GraphPane.XAxis.MinorTic.IsAllTics = false;
            GraphPane.XAxis.Title.IsVisible = true;
            GraphPane.XAxis.Title.Text = "Samples";
            GraphPane.XAxis.Scale.BaseTic = 0;
            GraphPane.YAxis.MinSpace = YAxisMinSpace;
            GraphPane.AxisChangeEvent += GraphPane_AxisChangeEvent;
            ZoomEvent += chart_ZoomEvent;
        }

        private void ResetWaveform()
        {
            sequenceIndex = 0;
            var paneCount = MasterPane.PaneList.Count;
            if (paneCount > 1)
            {
                MasterPane.PaneList.RemoveRange(1, paneCount - 1);
                SetLayout(PaneLayout.SquareColPreferred);
            }

            GraphPane.Margin.Top = overlayChannels ? DefaultPaneMargin : 0;
            GraphPane.Margin.Bottom = overlayChannels ? DefaultPaneMargin : 0;
            GraphPane.Margin.Left = overlayChannels ? DefaultPaneMargin : TilePaneHorizontalMargin;
            GraphPane.Margin.Right = overlayChannels ? DefaultPaneMargin : TilePaneHorizontalMargin;
            GraphPane.TitleGap = overlayChannels ? DefaultPaneTitleGap : 0;
            GraphPane.Title.IsVisible = !overlayChannels;
            GraphPane.YAxis.MinSpace = overlayChannels ? YAxisMinSpace : 0;
            GraphPane.YAxis.IsVisible = overlayChannels;
            GraphPane.XAxis.IsVisible = overlayChannels;
            GraphPane.CurveList.Clear();
            ResetColorCycle();
        }

        public bool OverlayChannels
        {
            get { return overlayChannels; }
            set
            {
                var changed = overlayChannels != value;
                overlayChannels = value;
                if (changed)
                {
                    ResetWaveform();
                }
            }
        }

        public int HistoryLength
        {
            get { return historyLength; }
            set { historyLength = value; }
        }

        public double ChannelOffset
        {
            get { return channelOffset; }
            set { channelOffset = value; }
        }

        public int WaveformBufferLength
        {
            get { return waveformBufferLength; }
            set { waveformBufferLength = value; }
        }

        public double XMin
        {
            get { return GraphPane.XAxis.Scale.Min; }
            set
            {
                foreach (var pane in MasterPane.PaneList)
                {
                    pane.XAxis.Scale.Min = value;
                }
                MasterPane.AxisChange();
                Invalidate();
            }
        }

        public double XMax
        {
            get { return GraphPane.XAxis.Scale.Max; }
            set
            {
                foreach (var pane in MasterPane.PaneList)
                {
                    pane.XAxis.Scale.Max = value;
                }
                MasterPane.AxisChange();
                Invalidate();
            }
        }

        public double YMin
        {
            get { return GraphPane.YAxis.Scale.Min; }
            set
            {
                foreach (var pane in MasterPane.PaneList)
                {
                    pane.YAxis.Scale.Min = value;
                }
                MasterPane.AxisChange();
                Invalidate();
            }
        }

        public double YMax
        {
            get { return GraphPane.YAxis.Scale.Max; }
            set
            {
                foreach (var pane in MasterPane.PaneList)
                {
                    pane.YAxis.Scale.Max = value;
                }
                MasterPane.AxisChange();
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
                if (changed) OnAutoScaleXChanged(EventArgs.Empty);
            }
        }

        public bool AutoScaleY
        {
            get { return autoScaleY; }
            set
            {
                var changed = autoScaleY != value;
                autoScaleY = value;
                if (changed) OnAutoScaleYChanged(EventArgs.Empty);
            }
        }

        public event EventHandler AutoScaleXChanged;

        public event EventHandler AutoScaleYChanged;

        protected virtual void OnAxisChanged(EventArgs e)
        {
            if (AutoScaleX)
            {
                foreach (var pane in MasterPane.PaneList)
                {
                    if (pane.CurveList.Count == 0) continue;
                    var points = (DownsampledPointPairList)pane.CurveList.First().Points;
                    pane.XAxis.Scale.Max = points.HistoryLength;
                    pane.XAxis.Scale.MaxAuto = AutoScaleX;
                }
            }

            if (!AutoScaleX) UpdateDataBounds();
        }

        private void UpdateDataBounds()
        {
            foreach (var pane in MasterPane.PaneList)
            {
                foreach (var curve in pane.CurveList)
                {
                    var points = (DownsampledPointPairList)curve.Points;
                    points.SetBounds(pane.XAxis.Scale.Min, pane.XAxis.Scale.Max, MaxSamplePoints);
                }
            }
        }

        public void UpdateWaveform(double[] samples, int rows, int columns)
        {
            if (values == null || values.Length != rows)
            {
                values = new DownsampledPointPairList[rows];
                ResetWaveform();
            }

            var graphPanes = MasterPane.PaneList;
            var seriesCount = graphPanes.Sum(pane => pane.CurveList.Count);
            for (int i = 0; i < values.Length; i++)
            {
                var seriesIndex = sequenceIndex * values.Length + i;
                if (seriesIndex < seriesCount)
                {
                    var curveList = graphPanes[seriesIndex % graphPanes.Count].CurveList;
                    var curveItem = curveList[seriesIndex / graphPanes.Count];
                    values[i] = (DownsampledPointPairList)curveItem.Points;
                }
                else values[i] = new DownsampledPointPairList();

                var points = values[i];
                points.HistoryLength = columns * HistoryLength;
                for (int j = 0; j < columns; j++)
                {
                    points.Add(samples[i * columns + j] + i * ChannelOffset);
                }

                if (AutoScaleX) points.SetBounds(0, points.HistoryLength, MaxSamplePoints);
            }

            if (sequenceIndex * values.Length >= seriesCount || values.Length > seriesCount)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    GraphPane pane;
                    if (overlayChannels) pane = GraphPane;
                    else
                    {
                        if (i < graphPanes.Count) pane = graphPanes[i];
                        else
                        {
                            pane = new GraphPane(GraphPane);
                            pane.Title.Text = i.ToString(CultureInfo.InvariantCulture);
                            pane.AxisChangeEvent += GraphPane_AxisChangeEvent;
                            pane.CurveList.Clear();
                            graphPanes.Add(pane);
                        }
                    }

                    var timeSeries = pane.CurveList;
                    var series = new LineItem(string.Empty, values[i], GetNextColor(), SymbolType.None);
                    series.Line.IsAntiAlias = true;
                    series.Line.IsOptimizedDraw = true;
                    series.Label.IsVisible = false;
                    timeSeries.Add(series);
                }

                if (!overlayChannels) SetLayout(PaneLayout.SquareColPreferred);
            }

            var requiredSeries = WaveformBufferLength * values.Length;
            if (requiredSeries < seriesCount)
            {
                if (overlayChannels)
                {
                    var timeSeries = GraphPane.CurveList;
                    timeSeries.RemoveRange(requiredSeries, timeSeries.Count - requiredSeries);
                }
                else
                {
                    var requiredCurves = requiredSeries / values.Length;
                    foreach (var pane in graphPanes)
                    {
                        pane.CurveList.RemoveRange(requiredCurves, pane.CurveList.Count - requiredCurves);
                    }
                }
            }

            sequenceIndex = (sequenceIndex + 1) % WaveformBufferLength;
        }

        private void chart_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            MasterPane.AxisChange();
        }

        private void GraphPane_AxisChangeEvent(GraphPane pane)
        {
            var xscale = pane.XAxis.Scale;
            var yscale = pane.YAxis.Scale;
            AutoScaleX = pane.XAxis.Scale.MaxAuto;
            AutoScaleY = pane.YAxis.Scale.MaxAuto;
            OnAxisChanged(EventArgs.Empty);
        }

        protected virtual void OnAutoScaleXChanged(EventArgs e)
        {
            AutoScaleAxis = autoScaleX || autoScaleY;
            foreach (var pane in MasterPane.PaneList)
            {
                pane.XAxis.Scale.MaxAuto = autoScaleX;
                pane.XAxis.Scale.MinAuto = autoScaleX;
            }
            if (AutoScaleAxis) Invalidate();

            var handler = AutoScaleXChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnAutoScaleYChanged(EventArgs e)
        {
            AutoScaleAxis = autoScaleX || autoScaleY;
            foreach (var pane in MasterPane.PaneList)
            {
                pane.YAxis.Scale.MaxAuto = autoScaleY;
                pane.YAxis.Scale.MinAuto = autoScaleY;
            }
            if (AutoScaleAxis) Invalidate();

            var handler = AutoScaleYChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
