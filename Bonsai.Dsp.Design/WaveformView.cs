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

namespace Bonsai.Dsp.Design
{
    public partial class WaveformView : UserControl
    {
        ToolStripTextBox yminTextBox;
        ToolStripTextBox ymaxTextBox;
        ToolStripTextBox xminTextBox;
        ToolStripTextBox xmaxTextBox;

        public WaveformView()
        {
            InitializeComponent();
            historyLengthNumericUpDown.Maximum = decimal.MaxValue;
            channelOffsetNumericUpDown.Minimum = decimal.MinValue;
            channelOffsetNumericUpDown.Maximum = decimal.MaxValue;
            bufferLengthNumericUpDown.Maximum = int.MaxValue;
            autoScaleXButton.Checked = true;
            autoScaleYButton.Checked = true;
            graph.GraphPane.AxisChangeEvent += GraphPane_AxisChangeEvent;

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

        protected WaveformGraph Graph
        {
            get { return graph; }
        }

        public Collection<int> SelectedChannels
        {
            get { return graph.SelectedChannels; }
        }

        public int SelectedPage
        {
            get { return graph.SelectedPage; }
            set
            {
                graph.SelectedPage = value;
                OnSelectedPageChanged(EventArgs.Empty);
            }
        }

        public int ChannelsPerPage
        {
            get { return graph.ChannelsPerPage; }
            set
            {
                graph.ChannelsPerPage = value;
                channelsPerPageNumericUpDown.Value = value;
            }
        }

        public bool OverlayChannels
        {
            get { return graph.OverlayChannels; }
            set { graph.OverlayChannels = value; }
        }

        public int HistoryLength
        {
            get { return graph.HistoryLength; }
            set
            {
                graph.HistoryLength = value;
                historyLengthNumericUpDown.Value = value;
            }
        }

        public double ChannelOffset
        {
            get { return graph.ChannelOffset; }
            set
            {
                graph.ChannelOffset = value;
                channelOffsetNumericUpDown.Value = (decimal)value;
            }
        }

        public int WaveformBufferLength
        {
            get { return graph.WaveformBufferLength; }
            set
            {
                graph.WaveformBufferLength = value;
                bufferLengthNumericUpDown.Value = value;
            }
        }

        public double XMin
        {
            get { return graph.XMin; }
            set { graph.XMin = value; }
        }

        public double XMax
        {
            get { return graph.XMax; }
            set { graph.XMax = value; }
        }

        public double YMin
        {
            get { return graph.YMin; }
            set { graph.YMin = value; }
        }

        public double YMax
        {
            get { return graph.YMax; }
            set { graph.YMax = value; }
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

        public event EventHandler SelectedPageChanged;

        protected virtual void OnAxisChanged(EventArgs e)
        {
            var handler = AxisChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedPageChanged(EventArgs e)
        {
            var handler = SelectedPageChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public virtual void EnsureWaveform(int rows, int columns)
        {
            graph.EnsureWaveformRows(rows);
        }

        public void UpdateWaveform(int channel, double[] samples, int rows, int columns)
        {
            graph.UpdateWaveform(channel, samples, rows, columns);
        }

        public virtual void UpdateWaveform(double[] samples, int rows, int columns)
        {
            graph.UpdateWaveform(samples, rows, columns);
        }

        public void InvalidateWaveform()
        {
            graph.Invalidate();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            var keyCode = keyData & Keys.KeyCode;
            var modifiers = keyData & Keys.Modifiers;
            if (modifiers == Keys.Control && keyCode == Keys.P)
            {
                graph.DoPrint();
            }

            if (modifiers == Keys.Control && keyCode == Keys.S)
            {
                graph.SaveAs();
            }

            if (keyCode == Keys.PageDown)
            {
                SelectedPage++;
            }

            if (keyCode == Keys.PageUp)
            {
                SelectedPage--;
            }

            return base.ProcessDialogKey(keyData);
        }

        private void graph_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            graph.MasterPane.AxisChange();
        }

        private bool graph_MouseMoveEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            double x, y;
            var pane = graph.MasterPane.FindChartRect(e.Location);
            if (pane != null)
            {
                pane.ReverseTransform(e.Location, out x, out y);
                cursorStatusLabel.Text = string.Format("Cursor: ({0:F0}, {1:G5})", x, y);
            }
            return false;
        }

        private void graph_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                statusStrip.Visible = !statusStrip.Visible;
            }
        }

        private void GraphPane_AxisChangeEvent(GraphPane pane)
        {
            var xscale = pane.XAxis.Scale;
            var yscale = pane.YAxis.Scale;
            autoScaleXButton.Checked = pane.XAxis.Scale.MaxAuto;
            autoScaleYButton.Checked = pane.YAxis.Scale.MaxAuto;
            xminStatusLabel.Text = xscale.Min.ToString("G5", CultureInfo.InvariantCulture);
            xmaxStatusLabel.Text = xscale.Max.ToString("G5", CultureInfo.InvariantCulture);
            yminStatusLabel.Text = yscale.Min.ToString("G5", CultureInfo.InvariantCulture);
            ymaxStatusLabel.Text = yscale.Max.ToString("G5", CultureInfo.InvariantCulture);
            OnAxisChanged(EventArgs.Empty);
        }

        private void autoScaleXButton_CheckedChanged(object sender, EventArgs e)
        {
            graph.AutoScaleX = autoScaleXButton.Checked;
            xminStatusLabel.Visible = !autoScaleXButton.Checked;
            xmaxStatusLabel.Visible = !autoScaleXButton.Checked;
        }

        private void autoScaleYButton_CheckedChanged(object sender, EventArgs e)
        {
            graph.AutoScaleY = autoScaleYButton.Checked;
            yminStatusLabel.Visible = !autoScaleYButton.Checked;
            ymaxStatusLabel.Visible = !autoScaleYButton.Checked;
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

        private void historyLengthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            HistoryLength = (int)historyLengthNumericUpDown.Value;
        }

        private void channelsPerPageNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            ChannelsPerPage = (int)channelsPerPageNumericUpDown.Value;
        }

        private void overlayModeSplitButton_Click(object sender, EventArgs e)
        {
            OverlayChannels = !OverlayChannels;
            overlayModeSplitButton.Image = !OverlayChannels
                ? Properties.Resources.OverlayGraphModeImage
                : Properties.Resources.OverlayGridModeImage;
        }

        private void graph_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                graph.ZoomOut(graph.GraphPane);
            }
        }
    }
}
