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
using System.Collections.ObjectModel;

namespace Bonsai.Design.Visualizers
{
    partial class RollingGraphView : UserControl
    {
        ToolStripTextBox minTextBox;
        ToolStripTextBox maxTextBox;
        ToolStripTextBox capacityTextBox;

        public RollingGraphView()
        {
            InitializeComponent();
            autoScaleButton.Checked = true;
            capacityTextBox = new ToolStripTextBox();
            capacityTextBox.LostFocus += capacityTextBox_LostFocus;
            InitializeEditableScale(capacityTextBox, capacityValueLabel);
            Graph.GraphPane.AxisChangeEvent += GraphPane_AxisChangeEvent;

            minTextBox = new ToolStripTextBox();
            maxTextBox = new ToolStripTextBox();
            minTextBox.LostFocus += minTextBox_LostFocus;
            maxTextBox.LostFocus += maxTextBox_LostFocus;
            InitializeEditableScale(minTextBox, minStatusLabel);
            InitializeEditableScale(maxTextBox, maxStatusLabel);
        }

        private void InitializeEditableScale(ToolStripTextBox textBox, ToolStripStatusLabel statusLabel)
        {
            statusLabel.Tag = textBox;
            textBox.Tag = statusLabel;
            textBox.LostFocus += editableTextBox_LostFocus;
            textBox.KeyDown += editableTextBox_KeyDown;
        }

        protected StatusStrip StatusStrip
        {
            get { return statusStrip; }
        }

        public RollingGraph Graph
        {
            get { return graph; }
        }

        public int NumSeries
        {
            get { return graph.NumSeries; }
            set { graph.NumSeries = value; }
        }

        public virtual int Capacity
        {
            get { return graph.Capacity; }
            set { graph.Capacity = value; }
        }

        public double Min
        {
            get { return graph.Min; }
            set { graph.Min = value; }
        }

        public double Max
        {
            get { return graph.Max; }
            set { graph.Max = value; }
        }

        public bool AutoScale
        {
            get { return autoScaleButton.Checked; }
            set { autoScaleButton.Checked = value; }
        }

        public event EventHandler AutoScaleChanged
        {
            add { autoScaleButton.CheckedChanged += value; }
            remove { autoScaleButton.CheckedChanged -= value; }
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

        protected override void OnLoad(EventArgs e)
        {
            graph.EnsureCapacity();
            base.OnLoad(e);
        }

        public virtual void AddValues(double index, params object[] values)
        {
            graph.AddValues(index, values);
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
            var capacity = graph.Capacity;
            var scale = pane.YAxis.Scale;
            autoScaleButton.Checked = pane.YAxis.Scale.MaxAuto;
            capacityValueLabel.Text = capacity.ToString(CultureInfo.InvariantCulture);
            minStatusLabel.Text = scale.Min.ToString("G5", CultureInfo.InvariantCulture);
            maxStatusLabel.Text = scale.Max.ToString("G5", CultureInfo.InvariantCulture);
            OnAxisChanged(EventArgs.Empty);
        }

        private void autoScaleButton_CheckedChanged(object sender, EventArgs e)
        {
            graph.AutoScale = autoScaleButton.Checked;
            minStatusLabel.Visible = !autoScaleButton.Checked;
            maxStatusLabel.Visible = !autoScaleButton.Checked;
        }

        private void editableTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                statusStrip.Select();
            }
        }

        private void capacityTextBox_LostFocus(object sender, EventArgs e)
        {
            int capacity;
            if (capacityTextBox.Text != capacityValueLabel.Text &&
                int.TryParse(capacityTextBox.Text, out capacity))
            {
                Capacity = capacity;
            }
        }

        private void maxTextBox_LostFocus(object sender, EventArgs e)
        {
            double max;
            if (maxTextBox.Text != maxStatusLabel.Text &&
                double.TryParse(maxTextBox.Text, out max))
            {
                Max = max;
            }
        }

        private void minTextBox_LostFocus(object sender, EventArgs e)
        {
            double min;
            if (minTextBox.Text != minStatusLabel.Text &&
                double.TryParse(minTextBox.Text, out min))
            {
                Min = min;
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

        private void graph_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                graph.ZoomOut(graph.GraphPane);
            }
        }
    }
}
