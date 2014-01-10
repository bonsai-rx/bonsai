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

namespace Bonsai.Dsp.Design
{
    public partial class WaveformGraph : UserControl
    {
        const float YAxisMinSpace = 50;

        int sequenceIndex;
        bool overlayChannels;
        DownsampledPointPairList[] values;
        ToolStripTextBox yminTextBox;
        ToolStripTextBox ymaxTextBox;
        ToolStripTextBox xminTextBox;
        ToolStripTextBox xmaxTextBox;

        public WaveformGraph()
        {
            InitializeComponent();
            overlayChannels = true;
            WaveformBufferLength = 1;
            historyLengthNumericUpDown.Maximum = decimal.MaxValue;
            channelOffsetNumericUpDown.Minimum = decimal.MinValue;
            channelOffsetNumericUpDown.Maximum = decimal.MaxValue;
            bufferLengthNumericUpDown.Maximum = int.MaxValue;
            autoScaleXButton.Checked = true;
            autoScaleYButton.Checked = true;
            chart.IsShowContextMenu = false;
            chart.GraphPane.XAxis.Type = AxisType.Linear;
            chart.GraphPane.XAxis.MinorTic.IsAllTics = false;
            chart.GraphPane.XAxis.Title.IsVisible = true;
            chart.GraphPane.XAxis.Title.Text = "Samples";
            chart.GraphPane.XAxis.Scale.BaseTic = 0;
            chart.GraphPane.YAxis.MinSpace = YAxisMinSpace;
            chart.GraphPane.AxisChangeEvent += GraphPane_AxisChangeEvent;

            xminTextBox = new ToolStripTextBox();
            xmaxTextBox = new ToolStripTextBox();
            xminTextBox.LostFocus += xminTextBox_LostFocus;
            xmaxTextBox.LostFocus += xmaxTextBox_LostFocus;
            InitializeEditableScale(xminTextBox, xmaxTextBox, xminStatusLabel, xmaxStatusLabel);

            yminTextBox = new ToolStripTextBox();
            ymaxTextBox = new ToolStripTextBox();
            yminTextBox.LostFocus += yminTextBox_LostFocus;
            ymaxTextBox.LostFocus += ymaxTextBox_LostFocus;
            InitializeEditableScale(yminTextBox, ymaxTextBox, yminStatusLabel, ymaxStatusLabel);
        }

        private void InitializeEditableScale(
            ToolStripTextBox minTextBox,
            ToolStripTextBox maxTextBox,
            ToolStripStatusLabel minStatusLabel,
            ToolStripStatusLabel maxStatusLabel)
        {
            minStatusLabel.Tag = minTextBox;
            maxStatusLabel.Tag = maxTextBox;
            minTextBox.Tag = minStatusLabel;
            maxTextBox.Tag = maxStatusLabel;
            minTextBox.LostFocus += editableTextBox_LostFocus;
            minTextBox.KeyDown += editableTextBox_KeyDown;
            maxTextBox.LostFocus += editableTextBox_LostFocus;
            maxTextBox.KeyDown += editableTextBox_KeyDown;
        }

        private void ResetWaveform()
        {
            sequenceIndex = 0;
            var paneCount = chart.MasterPane.PaneList.Count;
            if (paneCount > 1)
            {
                chart.MasterPane.PaneList.RemoveRange(1, paneCount - 1);
                chart.SetLayout(PaneLayout.SquareColPreferred);
            }

            chart.GraphPane.YAxis.MinSpace = overlayChannels ? YAxisMinSpace : 0;
            chart.GraphPane.YAxis.IsVisible = overlayChannels;
            chart.GraphPane.XAxis.IsVisible = overlayChannels;
            chart.GraphPane.CurveList.Clear();
            chart.ResetColorCycle();
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

        public double ChannelOffset { get; set; }

        public int WaveformBufferLength { get; set; }

        public double XMin
        {
            get { return chart.GraphPane.XAxis.Scale.Min; }
            set
            {
                foreach (var pane in chart.MasterPane.PaneList)
                {
                    pane.XAxis.Scale.Min = value;
                }
                chart.MasterPane.AxisChange();
                InvalidateWaveform();
            }
        }

        public double XMax
        {
            get { return chart.GraphPane.XAxis.Scale.Max; }
            set
            {
                foreach (var pane in chart.MasterPane.PaneList)
                {
                    pane.XAxis.Scale.Max = value;
                }
                chart.MasterPane.AxisChange();
                InvalidateWaveform();
            }
        }

        public double YMin
        {
            get { return chart.GraphPane.YAxis.Scale.Min; }
            set
            {
                foreach (var pane in chart.MasterPane.PaneList)
                {
                    pane.YAxis.Scale.Min = value;
                }
                chart.MasterPane.AxisChange();
                InvalidateWaveform();
            }
        }

        public double YMax
        {
            get { return chart.GraphPane.YAxis.Scale.Max; }
            set
            {
                foreach (var pane in chart.MasterPane.PaneList)
                {
                    pane.YAxis.Scale.Max = value;
                }
                chart.MasterPane.AxisChange();
                InvalidateWaveform();
            }
        }

        public bool AutoScaleX
        {
            get { return autoScaleXButton.Checked; }
            set { autoScaleXButton.Checked = value; }
        }

        public bool AutoScaleY
        {
            get { return autoScaleYButton.Checked; }
            set { autoScaleYButton.Checked = value; }
        }

        public event EventHandler AutoScaleXChanged
        {
            add { autoScaleXButton.CheckedChanged += value; }
            remove { autoScaleXButton.CheckedChanged -= value; }
        }

        public event EventHandler AutoScaleYChanged
        {
            add { autoScaleYButton.CheckedChanged += value; }
            remove { autoScaleYButton.CheckedChanged -= value; }
        }

        public event EventHandler AxisChanged;

        protected virtual void OnAxisChanged(EventArgs e)
        {
            if (AutoScaleX)
            {
                foreach (var pane in chart.MasterPane.PaneList)
                {
                    if (pane.CurveList.Count == 0) continue;
                    var points = (DownsampledPointPairList)pane.CurveList.First().Points;
                    pane.XAxis.Scale.Max = points.HistoryLength;
                    pane.XAxis.Scale.MaxAuto = AutoScaleX;
                }
            }

            if (!AutoScaleX) UpdateDataBounds();
            var handler = AxisChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void UpdateDataBounds()
        {
            foreach (var pane in chart.MasterPane.PaneList)
            {
                foreach (var curve in pane.CurveList)
                {
                    var pointList = (DownsampledPointPairList)curve.Points;
                    pointList.SetBounds(pane.XAxis.Scale.Min, pane.XAxis.Scale.Max, 1000);
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

            var graphPanes = chart.MasterPane.PaneList;
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
                values[i].HistoryLength = columns * (int)historyLengthNumericUpDown.Value;
                for (int j = 0; j < columns; j++)
                {
                    values[i].Add(samples[i * columns + j] + i * ChannelOffset);
                }

                if (AutoScaleX) values[i].SetBounds(0, values[i].List.Count, 1000);
            }

            if (sequenceIndex * values.Length >= seriesCount || values.Length > seriesCount)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    GraphPane pane;
                    if (overlayChannels) pane = chart.GraphPane;
                    else
                    {
                        if (i < graphPanes.Count) pane = graphPanes[i];
                        else
                        {
                            pane = new GraphPane(chart.GraphPane);
                            pane.CurveList.Clear();
                            graphPanes.Add(pane);
                        }
                    }

                    var timeSeries = pane.CurveList;
                    var series = new LineItem(string.Empty, values[i], chart.GetNextColor(), SymbolType.None);
                    series.Line.IsAntiAlias = true;
                    series.Line.IsOptimizedDraw = true;
                    series.Label.IsVisible = false;
                    timeSeries.Add(series);
                }

                if (!overlayChannels) chart.SetLayout(PaneLayout.SquareColPreferred);
            }

            var requiredSeries = WaveformBufferLength * values.Length;
            if (requiredSeries < seriesCount)
            {
                if (overlayChannels)
                {
                    var timeSeries = chart.GraphPane.CurveList;
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

        public void InvalidateWaveform()
        {
            chart.Invalidate();
        }

        private bool chart_MouseMoveEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            double x, y;
            var pane = chart.MasterPane.FindChartRect(e.Location);
            if (pane != null)
            {
                pane.ReverseTransform(e.Location, out x, out y);
                cursorStatusLabel.Text = string.Format("Cursor: ({0:F0}, {1:G5})", x, y);
            }
            return false;
        }

        private void chart_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                statusStrip.Visible = !statusStrip.Visible;
            }
        }

        private void chart_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            autoScaleXButton.Checked = false;
            autoScaleYButton.Checked = false;
        }

        private void GraphPane_AxisChangeEvent(GraphPane pane)
        {
            var xscale = pane.XAxis.Scale;
            var yscale = pane.YAxis.Scale;
            xminStatusLabel.Text = xscale.Min.ToString(CultureInfo.InvariantCulture);
            xmaxStatusLabel.Text = xscale.Max.ToString(CultureInfo.InvariantCulture);
            yminStatusLabel.Text = yscale.Min.ToString(CultureInfo.InvariantCulture);
            ymaxStatusLabel.Text = yscale.Max.ToString(CultureInfo.InvariantCulture);
            OnAxisChanged(EventArgs.Empty);
        }

        private void autoScaleXButton_CheckedChanged(object sender, EventArgs e)
        {
            chart.AutoScaleAxis = autoScaleXButton.Checked || autoScaleYButton.Checked;
            foreach (var pane in chart.MasterPane.PaneList)
            {
                pane.XAxis.Scale.MaxAuto = autoScaleXButton.Checked;
                pane.XAxis.Scale.MinAuto = autoScaleXButton.Checked;
            }
            xminStatusLabel.Visible = !autoScaleXButton.Checked;
            xmaxStatusLabel.Visible = !autoScaleXButton.Checked;
            if (chart.AutoScaleAxis) InvalidateWaveform();
        }

        private void autoScaleYButton_CheckedChanged(object sender, EventArgs e)
        {
            chart.AutoScaleAxis = autoScaleXButton.Checked || autoScaleYButton.Checked;
            foreach (var pane in chart.MasterPane.PaneList)
            {
                pane.YAxis.Scale.MaxAuto = autoScaleYButton.Checked;
                pane.YAxis.Scale.MinAuto = autoScaleYButton.Checked;
            }
            yminStatusLabel.Visible = !autoScaleYButton.Checked;
            ymaxStatusLabel.Visible = !autoScaleYButton.Checked;
            if (chart.AutoScaleAxis) InvalidateWaveform();
        }

        private void editableTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                statusStrip.Select();
            }
        }

        private void xmaxTextBox_LostFocus(object sender, EventArgs e)
        {
            double max;
            if (xmaxTextBox.Text != xmaxStatusLabel.Text &&
                double.TryParse(xmaxTextBox.Text, out max))
            {
                XMax = max;
            }
        }

        private void xminTextBox_LostFocus(object sender, EventArgs e)
        {
            double min;
            if (xminTextBox.Text != xminStatusLabel.Text &&
                double.TryParse(xminTextBox.Text, out min))
            {
                XMin = min;
            }
        }

        private void ymaxTextBox_LostFocus(object sender, EventArgs e)
        {
            double max;
            if (ymaxTextBox.Text != ymaxStatusLabel.Text &&
                double.TryParse(ymaxTextBox.Text, out max))
            {
                YMax = max;
            }
        }

        private void yminTextBox_LostFocus(object sender, EventArgs e)
        {
            double min;
            if (yminTextBox.Text != yminStatusLabel.Text &&
                double.TryParse(yminTextBox.Text, out min))
            {
                YMin = min;
            }
        }

        private void editableTextBox_LostFocus(object sender, EventArgs e)
        {
            var textBox = (ToolStripTextBox)sender;
            var statusLabel = (ToolStripStatusLabel)textBox.Tag;
            var labelIndex = statusStrip.Items.IndexOf(textBox);
            statusStrip.SuspendLayout();
            statusStrip.Items.Remove(textBox);
            statusStrip.Items.Insert(labelIndex, statusLabel);
            statusStrip.ResumeLayout();
        }

        private void editableStatusLabel_Click(object sender, EventArgs e)
        {
            var statusLabel = (ToolStripStatusLabel)sender;
            var textBox = (ToolStripTextBox)statusLabel.Tag;
            var labelIndex = statusStrip.Items.IndexOf(statusLabel);
            statusStrip.SuspendLayout();
            statusStrip.Items.Remove(statusLabel);
            statusStrip.Items.Insert(labelIndex, textBox);
            textBox.Size = statusLabel.Size;
            textBox.Text = statusLabel.Text;
            statusStrip.ResumeLayout();
            textBox.Focus();
        }

        private void channelOffsetNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            ChannelOffset = (double)channelOffsetNumericUpDown.Value;
        }

        private void bufferLengthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            WaveformBufferLength = (int)bufferLengthNumericUpDown.Value;
        }

        private void overlayModeSplitButton_Click(object sender, EventArgs e)
        {
            OverlayChannels = !OverlayChannels;
        }
    }
}
