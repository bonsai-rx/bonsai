using System;
using System.Windows.Forms;
using ZedGraph;
using System.Globalization;
using System.Collections.ObjectModel;

namespace Bonsai.Dsp.Design
{
    /// <summary>
    /// Represents a dynamic waveform oscilloscope style control used to display
    /// multi-dimensional matrices where each row represents an independent channel.
    /// </summary>
    public partial class WaveformView : UserControl
    {
        readonly ToolStripTextBox yminTextBox;
        readonly ToolStripTextBox ymaxTextBox;
        readonly ToolStripTextBox xminTextBox;
        readonly ToolStripTextBox xmaxTextBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaveformView"/> class.
        /// </summary>
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

        internal WaveformGraph Graph
        {
            get { return graph; }
        }

        /// <summary>
        /// Gets a collection of indices to the channels to display when the control
        /// is in overlay mode.
        /// </summary>
        public Collection<int> SelectedChannels
        {
            get { return graph.SelectedChannels; }
        }

        /// <summary>
        /// Gets or sets the currently selected channel page. Channels in the
        /// currently selected page will be the ones displayed in the graph.
        /// </summary>
        public int SelectedPage
        {
            get { return graph.SelectedPage; }
            set
            {
                graph.SelectedPage = value;
                OnSelectedPageChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of channels which should be included
        /// in a single page.
        /// </summary>
        public int ChannelsPerPage
        {
            get { return graph.ChannelsPerPage; }
            set
            {
                graph.ChannelsPerPage = value;
                channelsPerPageNumericUpDown.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to overlay the traces of all
        /// the channels in the page into a single waveform graph. If this value
        /// is <see langword="false"/>, channels will be displayed individually
        /// in separate graph panes.
        /// </summary>
        public bool OverlayChannels
        {
            get { return graph.OverlayChannels; }
            set { graph.OverlayChannels = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying how many previous data buffers to store
        /// and display in the graph.
        /// </summary>
        /// <remarks>
        /// Each buffer can contain multiple samples, which means the total number of
        /// samples displayed in the graph will be <c>HistoryLength * BufferLength</c>,
        /// where <c>BufferLength</c> is the number of samples per buffer.
        /// </remarks>
        public int HistoryLength
        {
            get { return graph.HistoryLength; }
            set
            {
                graph.HistoryLength = value;
                historyLengthNumericUpDown.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets a value which will be added to the samples of each channel,
        /// proportional to channel index, for the purposes of visualization.
        /// </summary>
        public double ChannelOffset
        {
            get { return graph.ChannelOffset; }
            set
            {
                graph.ChannelOffset = value;
                channelOffsetNumericUpDown.Value = (decimal)value;
            }
        }

        /// <summary>
        /// Gets or sets a value specifying how many previous traces to overlay for
        /// each channel.
        /// </summary>
        /// <remarks>
        /// This allows overlaying historical traces rather than appending them in time.
        /// </remarks>
        public int WaveformBufferLength
        {
            get { return graph.WaveformBufferLength; }
            set
            {
                graph.WaveformBufferLength = value;
                bufferLengthNumericUpDown.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the lower bound of the x-axis displayed in the graph.
        /// </summary>
        public double XMin
        {
            get { return graph.XMin; }
            set { graph.XMin = value; }
        }

        /// <summary>
        /// Gets or sets the upper bound of the x-axis displayed in the graph.
        /// </summary>
        public double XMax
        {
            get { return graph.XMax; }
            set { graph.XMax = value; }
        }

        /// <summary>
        /// Gets or sets the lower bound of the y-axis displayed in the graph.
        /// </summary>
        public double YMin
        {
            get { return graph.YMin; }
            set { graph.YMin = value; }
        }

        /// <summary>
        /// Gets or sets the upper bound of the y-axis displayed in the graph.
        /// </summary>
        public double YMax
        {
            get { return graph.YMax; }
            set { graph.YMax = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to compute the range of
        /// the x-axis automatically based on the range of the data that is
        /// included in the graph.
        /// </summary>
        public bool AutoScaleX
        {
            get { return autoScaleXButton.Checked; }
            set { autoScaleXButton.Checked = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to compute the range of
        /// the y-axis automatically based on the range of the data that is
        /// included in the graph.
        /// </summary>
        public bool AutoScaleY
        {
            get { return autoScaleYButton.Checked; }
            set { autoScaleYButton.Checked = value; }
        }

        /// <summary>
        /// Occurs when the <see cref="AutoScaleX"/> property changes.
        /// </summary>
        public event EventHandler AutoScaleXChanged
        {
            add { autoScaleXButton.CheckedChanged += value; }
            remove { autoScaleXButton.CheckedChanged -= value; }
        }

        /// <summary>
        /// Occurs when the <see cref="AutoScaleY"/> property changes.
        /// </summary>
        public event EventHandler AutoScaleYChanged
        {
            add { autoScaleYButton.CheckedChanged += value; }
            remove { autoScaleYButton.CheckedChanged -= value; }
        }

        /// <summary>
        /// Occurs when the scale ranges of the axes of the waveform view are
        /// recalculated.
        /// </summary>
        public event EventHandler AxisChanged;

        /// <summary>
        /// Occurs when the <see cref="SelectedPage"/> property changes.
        /// </summary>
        public event EventHandler SelectedPageChanged;

        /// <summary>
        /// Raises the <see cref="AxisChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// An <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnAxisChanged(EventArgs e)
        {
            AxisChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SelectedPageChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// An <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnSelectedPageChanged(EventArgs e)
        {
            SelectedPageChanged?.Invoke(this, e);
        }

        internal virtual void EnsureWaveform(int rows, int columns)
        {
            graph.EnsureWaveformRows(rows);
        }

        internal void UpdateWaveform(int channel, double[] samples, int rows, int columns)
        {
            graph.UpdateWaveform(channel, samples, rows, columns);
        }

        internal virtual void UpdateWaveform(double[] samples, int rows, int columns)
        {
            graph.UpdateWaveform(samples, rows, columns);
        }

        /// <summary>
        /// Invalidates the entire waveform graph and causes the underlying control
        /// to be redrawn.
        /// </summary>
        public void InvalidateWaveform()
        {
            graph.Invalidate();
        }

        /// <inheritdoc/>
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

            if (modifiers.HasFlag(Keys.Control) && keyCode == Keys.Oemplus)
            {
                var factor = modifiers.HasFlag(Keys.Shift) ? 10 : 1;
                HistoryLength += factor;
            }

            if (modifiers.HasFlag(Keys.Control) && keyCode == Keys.OemMinus)
            {
                var factor = modifiers.HasFlag(Keys.Shift) ? 10 : 1;
                HistoryLength = Math.Max(1, HistoryLength - factor);
            }

            return base.ProcessDialogKey(keyData);
        }

        private void graph_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {
            graph.MasterPane.AxisChange();
        }

        private bool graph_MouseMoveEvent(ZedGraphControl sender, MouseEventArgs e)
        {
            var pane = graph.MasterPane.FindChartRect(e.Location);
            if (pane != null)
            {
                pane.ReverseTransform(e.Location, out double x, out double y);
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
            if (xmaxTextBox.Text != xmaxStatusLabel.Text &&
                double.TryParse(xmaxTextBox.Text, out double max))
            {
                XMax = max;
            }
        }

        private void xminTextBox_LostFocus(object sender, EventArgs e)
        {
            if (xminTextBox.Text != xminStatusLabel.Text &&
                double.TryParse(xminTextBox.Text, out double min))
            {
                XMin = min;
            }
        }

        private void ymaxTextBox_LostFocus(object sender, EventArgs e)
        {
            if (ymaxTextBox.Text != ymaxStatusLabel.Text &&
                double.TryParse(ymaxTextBox.Text, out double max))
            {
                YMax = max;
            }
        }

        private void yminTextBox_LostFocus(object sender, EventArgs e)
        {
            if (yminTextBox.Text != yminStatusLabel.Text &&
                double.TryParse(yminTextBox.Text, out double min))
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
