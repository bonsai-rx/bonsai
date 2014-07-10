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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Bonsai.Design;
using System.Reactive.Linq;

namespace Bonsai.Dsp.Design
{
    public partial class WaveformGraph : ChartControl
    {
        const float TitleFontSize = 10;
        const float YAxisMinSpace = 50;
        const int MaxSamplePoints = 1000;
        const float DefaultPaneMargin = 10;
        const float DefaultPaneTitleGap = 0.5f;
        const float TileMasterPaneHorizontalMargin = 1;
        const float TilePaneVerticalMargin = 2;
        const float TilePaneHorizontalMargin = 2;
        const float TilePaneInnerGap = 1;
        static readonly TimeSpan SelectionRefreshInterval = TimeSpan.FromMilliseconds(30);

        bool autoScaleX;
        bool autoScaleY;
        int sequenceIndex;
        bool overlayChannels;
        int historyLength;
        double channelOffset;
        int waveformBufferLength;
        DownsampledPointPairList[] values;
        ObservableCollection<int> selectedChannels;
        IDisposable selectionNotifications;
        bool allowSelectionUpdate;

        public WaveformGraph()
        {
            autoScaleX = true;
            autoScaleY = true;
            overlayChannels = true;
            allowSelectionUpdate = true;
            WaveformBufferLength = 1;
            IsShowContextMenu = false;
            MasterPane.InnerPaneGap = 0;
            GraphPane.Border.Color = Color.Red;
            GraphPane.Title.FontSpec.IsBold = false;
            GraphPane.Title.FontSpec.Size = TitleFontSize;
            GraphPane.Title.Text = (0).ToString(CultureInfo.InvariantCulture);
            GraphPane.XAxis.Type = AxisType.Linear;
            GraphPane.XAxis.MinorTic.IsAllTics = false;
            GraphPane.XAxis.Title.IsVisible = true;
            GraphPane.XAxis.Title.Text = "Samples";
            GraphPane.XAxis.Scale.BaseTic = 0;
            selectedChannels = new ObservableCollection<int>();
            selectedChannels.CollectionChanged += selectedChannels_CollectionChanged;
            GraphPane.AxisChangeEvent += GraphPane_AxisChangeEvent;
            ZoomEvent += chart_ZoomEvent;
            InitializeReactiveEvents();
        }

        public Collection<int> SelectedChannels
        {
            get { return selectedChannels; }
        }

        private void InitializeReactiveEvents()
        {
            var scheduler = new ControlScheduler(this);
            var selectionDrag = (from mouseDown in MouseDown
                                 where !overlayChannels && MasterPane.FindChartRect(mouseDown.Location) == null
                                 let startRect = (Rectangle?)GetNormalizedRectangle(MasterPane.Rect, mouseDown.Location, mouseDown.Location)
                                 let previousSelection = MasterPane.PaneList.Select(pane => pane.Border.IsVisible).ToArray()
                                 select Observable.Return(startRect).Concat(
                                        (from mouseMove in MouseMove.TakeUntil(MouseUp)
                                                                    .Sample(SelectionRefreshInterval, scheduler)
                                         select (Rectangle?)GetNormalizedRectangle(MasterPane.Rect, mouseDown.Location, mouseMove.Location))
                                         .Concat(Observable.Return<Rectangle?>(null)))
                                         .Select(rect => new { previousSelection, rect }))
                                         .Merge();
            selectionNotifications = selectionDrag.Subscribe(xs => ProcessRubberBand(xs.previousSelection, xs.rect));
        }

        private void ProcessRubberBand(bool[] previousSelection, Rectangle? rect)
        {
            if (rect.HasValue)
            {
                for (int i = 0; i < MasterPane.PaneList.Count; i++)
                {
                    var pane = MasterPane.PaneList[i];
                    var selected = pane.Rect.IntersectsWith(rect.Value);
                    if (Control.ModifierKeys != Keys.Control || i >= previousSelection.Length)
                    {
                        pane.Border.IsVisible = selected;
                    }
                    else if (selected)
                    {
                        pane.Border.IsVisible = !previousSelection[i];
                    }
                }
            }
            else
            {
                allowSelectionUpdate = false;
                selectedChannels.Clear();
                for (int i = 0; i < MasterPane.PaneList.Count; i++)
                {
                    var pane = MasterPane.PaneList[i];
                    if (pane.Border.IsVisible) selectedChannels.Add(i);
                }

                UpdateSelection();
                allowSelectionUpdate = true;
            }

            UpdateRubberBand(rect.GetValueOrDefault(), Rectangle.Truncate(MasterPane.Rect));
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < MasterPane.PaneList.Count; i++)
            {
                var pane = MasterPane.PaneList[i];
                var selected = !overlayChannels && selectedChannels.Contains(i);
                pane.Border.IsVisible = selected;
            }
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

            MasterPane.InnerPaneGap = overlayChannels ? 0 : TilePaneInnerGap;
            MasterPane.Margin.Left = overlayChannels ? 0 : TileMasterPaneHorizontalMargin;
            MasterPane.Margin.Right = overlayChannels ? 0 : TileMasterPaneHorizontalMargin;
            GraphPane.Margin.Top = overlayChannels ? DefaultPaneMargin : TilePaneVerticalMargin;
            GraphPane.Margin.Bottom = overlayChannels ? DefaultPaneMargin : TilePaneVerticalMargin;
            GraphPane.Margin.Left = overlayChannels ? DefaultPaneMargin : TilePaneHorizontalMargin;
            GraphPane.Margin.Right = overlayChannels ? DefaultPaneMargin : TilePaneHorizontalMargin;
            GraphPane.TitleGap = overlayChannels ? DefaultPaneTitleGap : 0;
            GraphPane.Title.IsVisible = !overlayChannels;
            GraphPane.YAxis.MinSpace = overlayChannels ? YAxisMinSpace : 0;
            GraphPane.YAxis.IsVisible = overlayChannels;
            GraphPane.XAxis.IsVisible = overlayChannels;
            GraphPane.Border.IsVisible = !overlayChannels && selectedChannels.Contains(0);
            GraphPane.IsFontsScaled = overlayChannels;
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
                            pane.Border.IsVisible = selectedChannels.Contains(i);
                            pane.Title.Text = i.ToString(CultureInfo.InvariantCulture);
                            pane.AxisChangeEvent += GraphPane_AxisChangeEvent;
                            pane.CurveList.Clear();
                            graphPanes.Add(pane);
                        }
                    }

                    var timeSeries = pane.CurveList;
                    var series = new LineItem(string.Empty, values[i], GetColor(i), SymbolType.None);
                    series.Line.IsAntiAlias = true;
                    series.Line.IsOptimizedDraw = true;
                    series.Label.IsVisible = false;
                    series.IsVisible = !overlayChannels || selectedChannels.Count == 0 || selectedChannels.Contains(i);
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

        void selectedChannels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (allowSelectionUpdate)
            {
                UpdateSelection();
            }
        }

        private void chart_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            MasterPane.AxisChange();
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

            base.OnKeyDown(e);
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

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (selectionNotifications != null)
            {
                selectionNotifications.Dispose();
                selectionNotifications = null;
            }
            base.OnHandleDestroyed(e);
        }
    }
}
