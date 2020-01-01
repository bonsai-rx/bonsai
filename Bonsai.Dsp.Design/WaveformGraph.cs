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
using System.Reactive.Concurrency;

namespace Bonsai.Dsp.Design
{
    public partial class WaveformGraph : GraphControl
    {
        const float TitleFontSize = 10;
        const float YAxisMinSpace = 50;
        const int MinSamplePoints = 1000;
        const int SamplePointScale = 16000;
        const int MaxSamplePoints = 8000;
        const float DefaultPaneMargin = 10;
        const float DefaultPaneTitleGap = 0.5f;
        const float TileMasterPaneHorizontalMargin = 1;
        const float TilePaneVerticalMargin = 2;
        const float TilePaneHorizontalMargin = 2;
        const float TilePaneInnerGap = 1;
        const string TitleFormat = "Page {0}/{1}";
        static readonly TimeSpan SelectionRefreshInterval = TimeSpan.FromMilliseconds(30);

        bool autoScaleX;
        bool autoScaleY;
        SizeF scaleFactor;
        int[] sequenceIndices;
        int selectedPage;
        int channelsPerPage;
        int pageCount;
        int channelCount;
        int maxPointsPerChannel;
        bool overlayChannels;
        int historyLength;
        double channelOffset;
        int waveformBufferLength;
        DownsampledPointPairList[] values;
        ObservableCollection<int> selectedChannels;
        IDisposable selectionNotifications;
        bool allowSelectionUpdate;
        ReplayRandom random;
        bool replayDithering;

        public WaveformGraph()
        {
            autoScaleX = true;
            autoScaleY = true;
            channelsPerPage = 16;
            overlayChannels = true;
            allowSelectionUpdate = true;
            random = new ReplayRandom();
            WaveformBufferLength = 1;
            IsShowContextMenu = false;
            MasterPane.InnerPaneGap = 0;
            GraphPane.Border.Color = Color.Red;
            GraphPane.Title.FontSpec.IsBold = false;
            GraphPane.Title.FontSpec.Size = TitleFontSize * scaleFactor.Height;
            GraphPane.Title.Text = (0).ToString(CultureInfo.InvariantCulture);
            GraphPane.XAxis.Type = AxisType.Linear;
            GraphPane.XAxis.MinorTic.IsAllTics = false;
            GraphPane.XAxis.Title.IsVisible = true;
            GraphPane.XAxis.Title.Text = "Samples";
            GraphPane.XAxis.Scale.BaseTic = 0;
            selectedChannels = new ObservableCollection<int>();
            selectedChannels.CollectionChanged += selectedChannels_CollectionChanged;
            GraphPane.AxisChangeEvent += GraphPane_AxisChangeEvent;
            ZoomEvent += graph_ZoomEvent;
            InitializeReactiveEvents();
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            scaleFactor = factor;
            base.ScaleControl(factor, specified);
        }

        private void InitializeReactiveEvents()
        {
            var scheduler = new ControlScheduler(this);
            var selectionDrag = (from mouseDown in MouseDown
                                 where !overlayChannels &&
                                       mouseDown.Button == MouseButtons.Left &&
                                       MasterPane.FindChartRect(mouseDown.Location) == null
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                allowSelectionUpdate = false;
                selectedChannels.Clear();
                for (int i = 0; i < channelCount; i++)
                {
                    selectedChannels.Add(i);
                }
                UpdateSelection();
                allowSelectionUpdate = true;
            }

            if (e.Control && e.KeyCode == Keys.I)
            {
                allowSelectionUpdate = false;
                var selection = selectedChannels.ToList();
                selectedChannels.Clear();
                for (int i = 0; i < channelCount; i++)
                {
                    if (!selection.Contains(i))
                    {
                        selectedChannels.Add(i);
                    }
                }
                UpdateSelection();
                allowSelectionUpdate = true;
            }

            base.OnKeyDown(e);
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
                var extendSelection = Control.ModifierKeys == Keys.Control;
                if (!extendSelection) selectedChannels.Clear();
                for (int i = 0; i < MasterPane.PaneList.Count; i++)
                {
                    var pane = MasterPane.PaneList[i];
                    var channel = (int)pane.Tag;
                    if (extendSelection) selectedChannels.Remove(channel);
                    if (pane.Border.IsVisible) selectedChannels.Add(channel);
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
                var channel = (int)pane.Tag;
                var selected = !overlayChannels && selectedChannels.Contains(channel);
                pane.Border.IsVisible = selected;
            }
        }

        private void ResetWaveform()
        {
            var paneCount = MasterPane.PaneList.Count;
            MasterPane.PaneList.RemoveRange(1, paneCount - 1);
            sequenceIndices = new int[values == null ? 0 : values.Length];

            MasterPane.Title.IsVisible = pageCount > 1;
            MasterPane.Title.Text = string.Format(TitleFormat, selectedPage + 1, pageCount);
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

            if (!overlayChannels && values != null)
            {
                var graphPanes = MasterPane.PaneList;
                if (graphPanes.Count != values.Length || GraphPane.Tag == null)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        var pane = GraphPane;
                        if (i > 0)
                        {
                            pane = new GraphPane(pane);
                            pane.AxisChangeEvent += GraphPane_AxisChangeEvent;
                            graphPanes.Add(pane);
                        }

                        var channel = selectedPage * channelsPerPage + i;
                        pane.Border.IsVisible = selectedChannels.Contains(channel);
                        pane.Title.Text = channel.ToString(CultureInfo.InvariantCulture);
                        pane.CurveList.Clear();
                        pane.Tag = channel;
                    }
                }
            }

            if (!overlayChannels && pageCount > 1)
            {
                var squareSize = (int)Math.Ceiling(Math.Sqrt(channelsPerPage));
                SetLayout(squareSize, squareSize);
            }
            else SetLayout(PaneLayout.SquareColPreferred);
        }

        public int SelectedPage
        {
            get { return selectedPage; }
            set
            {
                var page = Math.Max(0, Math.Min(value, pageCount - 1));
                var changed = selectedPage != page;
                selectedPage = page;
                if (changed)
                {
                    ResetWaveform();
                }
            }
        }

        public int ChannelsPerPage
        {
            get { return channelsPerPage; }
            set
            {
                var changed = channelsPerPage != value;
                channelsPerPage = value;
                if (changed)
                {
                    ResetWaveform();
                }
            }
        }

        public Collection<int> SelectedChannels
        {
            get { return selectedChannels; }
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

        private bool FilterChannels
        {
            get { return overlayChannels && selectedChannels.Count > 0; }
        }

        internal int ChannelCount
        {
            get { return channelCount; }
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
            var record = replayDithering;
            foreach (var pane in MasterPane.PaneList)
            {
                foreach (var curve in pane.CurveList)
                {
                    if (record)
                    {
                        random.Mode = ReplayMode.Recording;
                        record = false;
                    }
                    else if (replayDithering) random.Mode = ReplayMode.Replaying;
                    
                    var points = (DownsampledPointPairList)curve.Points;
                    points.SetBounds(pane.XAxis.Scale.Min, pane.XAxis.Scale.Max, maxPointsPerChannel);
                }
            }

            random.Mode = ReplayMode.None;
        }

        public void EnsureWaveformRows(int channels)
        {
            var channelsPerPage = ChannelsPerPage;
            if (FilterChannels) channels = selectedChannels.Count;
            var remainderChannels = channels % channelsPerPage;
            var channelCountChanged = channelCount != channels;
            pageCount = channels / channelsPerPage;
            if (remainderChannels != 0) pageCount++;
            selectedPage = Math.Min(selectedPage, pageCount - 1);
            channelCount = channels;
            if (pageCount > 1)
            {
                var lastPage = selectedPage >= pageCount - 1;
                channels = lastPage && remainderChannels != 0 ? remainderChannels : channelsPerPage;
            }

            if (values == null || values.Length != channels || channelCountChanged)
            {
                values = new DownsampledPointPairList[channels];
                maxPointsPerChannel = Math.Max(MinSamplePoints, Math.Min(SamplePointScale / channelCount, MaxSamplePoints));
                ResetWaveform();
            }
        }

        private void InsertTimeSeries(int paneIndex, int channel)
        {
            GraphPane pane;
            var graphPanes = MasterPane.PaneList;
            if (overlayChannels) pane = GraphPane;
            else pane = graphPanes[paneIndex];

            var timeSeries = pane.CurveList;
            var series = new LineItem(string.Empty, values[paneIndex], GetColor(channel), SymbolType.None);
            series.Line.IsAntiAlias = true;
            series.Line.IsOptimizedDraw = true;
            series.Label.IsVisible = false;
            timeSeries.Add(series);
        }

        private static void EnsureMaxSeries(CurveList timeSeries, int maxSeries)
        {
            timeSeries.RemoveRange(0, timeSeries.Count - maxSeries);
        }

        public void UpdateWaveform(int channel, double[] samples, int rows, int columns)
        {
            var channelLabel = channel;
            var graphPanes = MasterPane.PaneList;
            if (FilterChannels) channel = selectedChannels.IndexOf(channel);
            var channelPage = channel / channelsPerPage;
            if (channelPage != selectedPage || channel < 0) return;
            var channelPane = channel % channelsPerPage;
            var seriesPane = !overlayChannels && channelPane < graphPanes.Count ? graphPanes[channelPane] : graphPanes[0];
            var seriesCount = seriesPane.CurveList.Count;

            var setBounds = AutoScaleX;
            var seriesIndex = sequenceIndices[channelPane];
            if (!overlayChannels && seriesIndex < seriesCount)
            {
                var curveList = graphPanes[channelPane].CurveList;
                var curveItem = curveList[seriesIndex];
                values[channelPane] = (DownsampledPointPairList)curveItem.Points;
            }
            else
            {
                values[channelPane] = new DownsampledPointPairList(random);
                setBounds = true;
            }

            var points = values[channelPane];
            points.HistoryLength = columns * HistoryLength;
            for (int j = 0; j < samples.Length; j++)
            {
                points.Add(samples[j] + channelPane * ChannelOffset);
            }

            if (setBounds) points.SetBounds(0, points.HistoryLength, maxPointsPerChannel);
            if (overlayChannels || seriesIndex >= seriesCount)
            {
                InsertTimeSeries(channelPane, channelLabel);
            }

            var maxSeries = WaveformBufferLength;
            if (seriesCount >= maxSeries)
            {
                if (overlayChannels)
                {
                    EnsureMaxSeries(GraphPane.CurveList, maxSeries);
                }
                else
                {
                    EnsureMaxSeries(graphPanes[channelPane].CurveList, maxSeries);
                }
            }

            sequenceIndices[channelPane] = (sequenceIndices[channelPane] + 1) % WaveformBufferLength;
        }

        public void UpdateWaveform(double[] samples, int rows, int columns)
        {
            replayDithering = true;
            var filterChannels = FilterChannels;
            EnsureWaveformRows(rows);

            var graphPanes = MasterPane.PaneList;
            var seriesCount = graphPanes.Sum(pane => pane.CurveList.Count);
            for (int i = 0; i < values.Length; i++)
            {
                var setBounds = AutoScaleX;
                var seriesIndex = sequenceIndices[i] * values.Length + i;
                if (seriesIndex < seriesCount)
                {
                    var curveList = graphPanes[seriesIndex % graphPanes.Count].CurveList;
                    var curveItem = curveList[seriesIndex / graphPanes.Count];
                    values[i] = (DownsampledPointPairList)curveItem.Points;
                }
                else
                {
                    values[i] = new DownsampledPointPairList(random);
                    setBounds = true;
                }

                var points = values[i];
                random.Mode = i == 0 ? ReplayMode.Recording : ReplayMode.Replaying;
                points.HistoryLength = columns * HistoryLength;
                var channel = selectedPage * channelsPerPage + i;
                if (filterChannels) channel = selectedChannels[channel];
                for (int j = 0; j < columns; j++)
                {
                    points.Add(samples[channel * columns + j] + i * ChannelOffset);
                }

                if (setBounds) points.SetBounds(0, points.HistoryLength, maxPointsPerChannel);
            }

            random.Mode = ReplayMode.None;
            if (sequenceIndices[0] * values.Length >= seriesCount || values.Length > seriesCount)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var channel = selectedPage * channelsPerPage + i;
                    if (filterChannels) channel = selectedChannels[channel];
                    InsertTimeSeries(i, channel);
                }
            }

            var maxSeries = WaveformBufferLength * values.Length;
            if (seriesCount >= maxSeries)
            {
                if (overlayChannels)
                {
                    EnsureMaxSeries(GraphPane.CurveList, maxSeries);
                }
                else
                {
                    var maxPaneSeries = maxSeries / values.Length;
                    foreach (var pane in graphPanes)
                    {
                        EnsureMaxSeries(pane.CurveList, maxPaneSeries);
                    }
                }
            }

            for (int i = 0; i < sequenceIndices.Length; i++)
            {
                sequenceIndices[i] = (sequenceIndices[i] + 1) % WaveformBufferLength;
            }
        }

        void selectedChannels_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (allowSelectionUpdate && values != null)
            {
                UpdateSelection();
            }
        }

        private void graph_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            MasterPane.AxisChange();
        }

        private void GraphPane_AxisChangeEvent(GraphPane pane)
        {
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
