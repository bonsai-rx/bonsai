using System;
using System.Windows.Forms;
using ZedGraph;
using System.Globalization;

namespace Bonsai.Design.Visualizers
{
    partial class RollingGraphView : UserControl
    {
        readonly ToolStripEditableLabel minEditableLabel;
        readonly ToolStripEditableLabel maxEditableLabel;
        readonly ToolStripEditableLabel capacityEditableLabel;

        public RollingGraphView()
        {
            InitializeComponent();
            autoScaleButton.Checked = true;
            capacityEditableLabel = new ToolStripEditableLabel(capacityValueLabel, OnCapacityEdit);
            minEditableLabel = new ToolStripEditableLabel(minStatusLabel, OnMinEdit);
            maxEditableLabel = new ToolStripEditableLabel(maxStatusLabel, OnMaxEdit);
            Graph.GraphPane.AxisChangeEvent += GraphPane_AxisChangeEvent;
            components.Add(capacityEditableLabel);
            components.Add(minEditableLabel);
            components.Add(maxEditableLabel);
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
            set { graph.EnsureCapacity(value); }
        }

        public virtual int Capacity
        {
            get { return graph.Capacity; }
            set { graph.Capacity = value; }
        }

        public bool CanEditCapacity
        {
            get { return capacityEditableLabel.Enabled; }
            set { capacityEditableLabel.Enabled = value; }
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

        public bool AutoScaleVisible
        {
            get { return autoScaleButton.Visible; }
            set
            {
                autoScaleButton.Visible = value;
                minEditableLabel.Enabled = value;
                maxEditableLabel.Enabled = value;
            }
        }

        public event EventHandler AutoScaleChanged
        {
            add { autoScaleButton.CheckedChanged += value; }
            remove { autoScaleButton.CheckedChanged -= value; }
        }

        public event EventHandler AxisChanged;

        protected virtual void OnAxisChanged(EventArgs e)
        {
            AxisChanged?.Invoke(this, e);
        }

        protected override void OnLoad(EventArgs e)
        {
            graph.EnsureCapacity(NumSeries);
            base.OnLoad(e);
        }

        public void AddValues(string index, params double[] values)
        {
            graph.AddValues(0, index, values);
        }

        public virtual void AddValues(double index, params double[] values)
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

        private void OnCapacityEdit(string text)
        {
            if (int.TryParse(text, out int capacity))
            {
                Capacity = capacity;
            }
        }

        private void OnMinEdit(string text)
        {
            if (double.TryParse(text, out double min))
            {
                Min = min;
            }
        }

        private void OnMaxEdit(string text)
        {
            if (double.TryParse(text, out double max))
            {
                Max = max;
            }
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
