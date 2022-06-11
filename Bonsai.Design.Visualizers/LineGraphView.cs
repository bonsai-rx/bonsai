using System;
using System.Windows.Forms;
using ZedGraph;
using System.Globalization;

namespace Bonsai.Design.Visualizers
{
    partial class LineGraphView : UserControl
    {
        readonly ToolStripEditableLabel minEditableLabelX;
        readonly ToolStripEditableLabel maxEditableLabelX;
        readonly ToolStripEditableLabel minEditableLabelY;
        readonly ToolStripEditableLabel maxEditableLabelY;
        readonly ToolStripEditableLabel capacityEditableLabel;

        public LineGraphView()
        {
            InitializeComponent();
            autoScaleButtonX.Checked = true;
            autoScaleButtonY.Checked = true;
            capacityEditableLabel = new ToolStripEditableLabel(capacityValueLabel, OnCapacityEdit);
            minEditableLabelX = new ToolStripEditableLabel(minStatusLabelX, OnXMinEdit);
            maxEditableLabelX = new ToolStripEditableLabel(maxStatusLabelX, OnXMaxEdit);
            minEditableLabelY = new ToolStripEditableLabel(minStatusLabelY, OnYMinEdit);
            maxEditableLabelY = new ToolStripEditableLabel(maxStatusLabelY, OnYMaxEdit);
            Graph.GraphPane.AxisChangeEvent += GraphPane_AxisChangeEvent;
            components.Add(capacityEditableLabel);
            components.Add(minEditableLabelX);
            components.Add(maxEditableLabelX);
            components.Add(minEditableLabelY);
            components.Add(maxEditableLabelY);
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
            get { return autoScaleButtonX.Checked; }
            set { autoScaleButtonX.Checked = value; }
        }

        public bool AutoScaleY
        {
            get { return autoScaleButtonY.Checked; }
            set { autoScaleButtonY.Checked = value; }
        }

        public bool AutoScaleXVisible
        {
            get { return autoScaleButtonX.Visible; }
            set
            {
                autoScaleButtonX.Visible = value;
                minEditableLabelX.Enabled = value;
                maxEditableLabelX.Enabled = value;
            }
        }

        public bool AutoScaleYVisible
        {
            get { return autoScaleButtonY.Visible; }
            set
            {
                autoScaleButtonY.Visible = value;
                minEditableLabelY.Enabled = value;
                maxEditableLabelY.Enabled = value;
            }
        }

        public event EventHandler AutoScaleXChanged
        {
            add { autoScaleButtonX.CheckedChanged += value; }
            remove { autoScaleButtonX.CheckedChanged -= value; }
        }

        public event EventHandler AutoScaleYChanged
        {
            add { autoScaleButtonY.CheckedChanged += value; }
            remove { autoScaleButtonY.CheckedChanged -= value; }
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

        public virtual void AddValues(params PointPair[] values)
        {
            graph.AddValues(values);
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
                cursorStatusLabel.Text = string.Format("Cursor: ({0:G5}, {1:G5})", x, y);
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
            var scaleX = pane.XAxis.Scale;
            var scaleY = pane.YAxis.Scale;
            autoScaleButtonX.Checked = pane.XAxis.Scale.MaxAuto;
            autoScaleButtonY.Checked = pane.YAxis.Scale.MaxAuto;
            capacityValueLabel.Text = capacity.ToString(CultureInfo.InvariantCulture);
            minStatusLabelX.Text = scaleX.Min.ToString("G5", CultureInfo.InvariantCulture);
            maxStatusLabelX.Text = scaleX.Max.ToString("G5", CultureInfo.InvariantCulture);
            minStatusLabelY.Text = scaleY.Min.ToString("G5", CultureInfo.InvariantCulture);
            maxStatusLabelY.Text = scaleY.Max.ToString("G5", CultureInfo.InvariantCulture);
            OnAxisChanged(EventArgs.Empty);
        }

        private void autoScaleButtonX_CheckedChanged(object sender, EventArgs e)
        {
            graph.AutoScaleX = autoScaleButtonX.Checked;
            minStatusLabelX.Visible = !autoScaleButtonX.Checked;
            maxStatusLabelX.Visible = !autoScaleButtonX.Checked;
        }

        private void autoScaleButtonY_CheckedChanged(object sender, EventArgs e)
        {
            graph.AutoScaleY = autoScaleButtonY.Checked;
            minStatusLabelY.Visible = !autoScaleButtonY.Checked;
            maxStatusLabelY.Visible = !autoScaleButtonY.Checked;
        }

        private void OnCapacityEdit(string text)
        {
            if (int.TryParse(text, out int capacity))
            {
                Capacity = capacity;
            }
        }

        private void OnXMinEdit(string text)
        {
            if (double.TryParse(text, out double min))
            {
                XMin = min;
            }
        }

        private void OnXMaxEdit(string text)
        {
            if (double.TryParse(text, out double max))
            {
                XMax = max;
            }
        }

        private void OnYMinEdit(string text)
        {
            if (double.TryParse(text, out double min))
            {
                YMin = min;
            }
        }

        private void OnYMaxEdit(string text)
        {
            if (double.TryParse(text, out double max))
            {
                YMax = max;
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
