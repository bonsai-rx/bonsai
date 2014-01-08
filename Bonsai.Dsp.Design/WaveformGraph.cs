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
        int sequenceIndex;
        PointPairList[] values;
        ToolStripTextBox yminTextBox;
        ToolStripTextBox ymaxTextBox;
        ToolStripTextBox xminTextBox;
        ToolStripTextBox xmaxTextBox;

        public WaveformGraph()
        {
            WaveformBufferLength = 1;
            InitializeComponent();
            chart.IsShowContextMenu = false;
            autoScaleXButton.Checked = true;
            autoScaleYButton.Checked = true;
            chart.GraphPane.XAxis.Type = AxisType.Ordinal;
            chart.GraphPane.XAxis.MinorTic.IsAllTics = false;
            chart.GraphPane.XAxis.Title.IsVisible = true;
            chart.GraphPane.XAxis.Title.Text = "Samples";
            chart.GraphPane.XAxis.Scale.BaseTic = 0;
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

        public double ChannelOffset { get; set; }

        public int WaveformBufferLength { get; set; }

        public double XMin
        {
            get { return chart.GraphPane.XAxis.Scale.Min; }
            set
            {
                chart.GraphPane.XAxis.Scale.Min = value;
                chart.GraphPane.AxisChange();
                InvalidateWaveform();
            }
        }

        public double XMax
        {
            get { return chart.GraphPane.XAxis.Scale.Max; }
            set
            {
                chart.GraphPane.XAxis.Scale.Max = value;
                chart.GraphPane.AxisChange();
                InvalidateWaveform();
            }
        }

        public double YMin
        {
            get { return chart.GraphPane.YAxis.Scale.Min; }
            set
            {
                chart.GraphPane.YAxis.Scale.Min = value;
                chart.GraphPane.AxisChange();
                InvalidateWaveform();
            }
        }

        public double YMax
        {
            get { return chart.GraphPane.YAxis.Scale.Max; }
            set
            {
                chart.GraphPane.YAxis.Scale.Max = value;
                chart.GraphPane.AxisChange();
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
            var handler = AxisChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void UpdateWaveform(double[] samples, int rows, int columns)
        {
            if (values == null || values.Length != rows)
            {
                values = new PointPairList[rows];
            }

            var timeSeries = chart.GraphPane.CurveList;
            for (int i = 0; i < values.Length; i++)
            {
                var seriesIndex = sequenceIndex * values.Length + i;
                if (seriesIndex < timeSeries.Count)
                {
                    values[i] = (PointPairList)timeSeries[seriesIndex].Points;
                    values[i].Clear();
                }
                else values[i] = new PointPairList();
                for (int j = 0; j < columns; j++)
                {
                    values[i].Add(j, samples[i * columns + j] + i * ChannelOffset);
                }
            }

            if (sequenceIndex * values.Length >= timeSeries.Count || values.Length > timeSeries.Count)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    var series = new LineItem(string.Empty, values[i], chart.GetNextColor(), SymbolType.None);
                    series.Line.IsAntiAlias = true;
                    series.Line.IsOptimizedDraw = true;
                    series.Label.IsVisible = false;
                    timeSeries.Add(series);
                }
            }

            var requiredSeries = WaveformBufferLength * values.Length;
            if (requiredSeries < timeSeries.Count)
            {
                timeSeries.RemoveRange(requiredSeries, timeSeries.Count - requiredSeries);
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
            chart.GraphPane.ReverseTransform(e.Location, out x, out y);
            cursorStatusLabel.Text = string.Format("Cursor: ({0:F0}, {1:G5})", x, y);
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
            chart.GraphPane.XAxis.Scale.MaxAuto = autoScaleXButton.Checked;
            chart.GraphPane.XAxis.Scale.MinAuto = autoScaleXButton.Checked;
            xminStatusLabel.Visible = !autoScaleXButton.Checked;
            xmaxStatusLabel.Visible = !autoScaleXButton.Checked;
            if (chart.AutoScaleAxis) InvalidateWaveform();
        }

        private void autoScaleYButton_CheckedChanged(object sender, EventArgs e)
        {
            chart.AutoScaleAxis = autoScaleXButton.Checked || autoScaleYButton.Checked;
            chart.GraphPane.YAxis.Scale.MaxAuto = autoScaleYButton.Checked;
            chart.GraphPane.YAxis.Scale.MinAuto = autoScaleYButton.Checked;
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
    }
}
