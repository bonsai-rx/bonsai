namespace Bonsai.Design.Visualizers
{
    partial class TimeSeriesView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.cursorStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.capacityStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.capacityValueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.scaleStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.minStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.maxStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.autoScaleButton = new System.Windows.Forms.ToolStripButton();
            this.chart = new Bonsai.Design.Visualizers.RollingGraph();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cursorStatusLabel,
            this.capacityStatusLabel,
            this.capacityValueLabel,
            this.scaleStatusLabel,
            this.minStatusLabel,
            this.maxStatusLabel,
            this.autoScaleButton});
            this.statusStrip.Location = new System.Drawing.Point(0, 218);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(400, 22);
            this.statusStrip.TabIndex = 1;
            this.statusStrip.Text = "statusStrip1";
            this.statusStrip.Visible = false;
            // 
            // cursorStatusLabel
            // 
            this.cursorStatusLabel.Name = "cursorStatusLabel";
            this.cursorStatusLabel.Size = new System.Drawing.Size(45, 17);
            this.cursorStatusLabel.Text = "Cursor:";
            // 
            // capacityStatusLabel
            // 
            this.capacityStatusLabel.Name = "capacityStatusLabel";
            this.capacityStatusLabel.Size = new System.Drawing.Size(47, 17);
            this.capacityStatusLabel.Text = "Capacity:";
            // 
            // capacityValueLabel
            // 
            this.capacityValueLabel.Name = "capacityValueLabel";
            this.capacityValueLabel.Size = new System.Drawing.Size(12, 17);
            this.capacityValueLabel.Text = "count";
            this.capacityValueLabel.Click += new System.EventHandler(this.editableStatusLabel_Click);
            // 
            // scaleStatusLabel
            // 
            this.scaleStatusLabel.Name = "scaleStatusLabel";
            this.scaleStatusLabel.Size = new System.Drawing.Size(47, 17);
            this.scaleStatusLabel.Text = "Scale:";
            // 
            // minStatusLabel
            // 
            this.minStatusLabel.Name = "minStatusLabel";
            this.minStatusLabel.Size = new System.Drawing.Size(13, 17);
            this.minStatusLabel.Text = "min";
            this.minStatusLabel.Visible = false;
            this.minStatusLabel.Click += new System.EventHandler(this.editableStatusLabel_Click);
            // 
            // maxStatusLabel
            // 
            this.maxStatusLabel.Name = "maxStatusLabel";
            this.maxStatusLabel.Size = new System.Drawing.Size(14, 17);
            this.maxStatusLabel.Text = "Max";
            this.maxStatusLabel.Visible = false;
            this.maxStatusLabel.Click += new System.EventHandler(this.editableStatusLabel_Click);
            // 
            // autoScaleButton
            // 
            this.autoScaleButton.Checked = true;
            this.autoScaleButton.CheckOnClick = true;
            this.autoScaleButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoScaleButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.autoScaleButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.autoScaleButton.Name = "autoScaleButton";
            this.autoScaleButton.Size = new System.Drawing.Size(35, 20);
            this.autoScaleButton.Text = "auto";
            this.autoScaleButton.CheckedChanged += new System.EventHandler(this.autoScaleButton_CheckedChanged);
            // 
            // chart
            // 
            this.chart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart.Location = new System.Drawing.Point(0, 0);
            this.chart.Name = "chart";
            this.chart.ScrollGrace = 0D;
            this.chart.ScrollMaxX = 0D;
            this.chart.ScrollMaxY = 0D;
            this.chart.ScrollMaxY2 = 0D;
            this.chart.ScrollMinX = 0D;
            this.chart.ScrollMinY = 0D;
            this.chart.ScrollMinY2 = 0D;
            this.chart.Size = new System.Drawing.Size(400, 218);
            this.chart.TabIndex = 2;
            this.chart.ZoomEvent += new ZedGraph.ZedGraphControl.ZoomEventHandler(this.chart_ZoomEvent);
            this.chart.MouseMoveEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.chart_MouseMoveEvent);
            this.chart.KeyDown += new System.Windows.Forms.KeyEventHandler(this.chart_KeyDown);
            this.chart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chart_MouseClick);
            // 
            // WaveformGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chart);
            this.Controls.Add(this.statusStrip);
            this.Name = "WaveformGraph";
            this.Size = new System.Drawing.Size(400, 240);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripButton autoScaleButton;
        private RollingGraph chart;
        private System.Windows.Forms.ToolStripStatusLabel cursorStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel scaleStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel minStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel maxStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel capacityStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel capacityValueLabel;
    }
}
