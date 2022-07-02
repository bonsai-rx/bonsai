namespace Bonsai.Design.Visualizers
{
    partial class LineGraphView
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
            this.scaleStatusLabelX = new System.Windows.Forms.ToolStripStatusLabel();
            this.minStatusLabelX = new System.Windows.Forms.ToolStripStatusLabel();
            this.maxStatusLabelX = new System.Windows.Forms.ToolStripStatusLabel();
            this.scaleStatusLabelY = new System.Windows.Forms.ToolStripStatusLabel();
            this.minStatusLabelY = new System.Windows.Forms.ToolStripStatusLabel();
            this.maxStatusLabelY = new System.Windows.Forms.ToolStripStatusLabel();
            this.autoScaleButtonX = new System.Windows.Forms.ToolStripButton();
            this.autoScaleButtonY = new System.Windows.Forms.ToolStripButton();
            this.graph = new Bonsai.Design.Visualizers.LineGraph();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cursorStatusLabel,
            this.capacityStatusLabel,
            this.capacityValueLabel,
            this.scaleStatusLabelX,
            this.minStatusLabelX,
            this.maxStatusLabelX,
            this.autoScaleButtonX,
            this.scaleStatusLabelY,
            this.minStatusLabelY,
            this.maxStatusLabelY,
            this.autoScaleButtonY});
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
            // 
            // scaleStatusLabelX
            // 
            this.scaleStatusLabelX.Name = "statusLabelX";
            this.scaleStatusLabelX.Size = new System.Drawing.Size(47, 17);
            this.scaleStatusLabelX.Text = "X:";
            // 
            // minStatusLabelX
            // 
            this.minStatusLabelX.Name = "minStatusLabelX";
            this.minStatusLabelX.Size = new System.Drawing.Size(13, 17);
            this.minStatusLabelX.Text = "min";
            this.minStatusLabelX.Visible = false;
            // 
            // maxStatusLabelX
            // 
            this.maxStatusLabelX.Name = "maxStatusLabelX";
            this.maxStatusLabelX.Size = new System.Drawing.Size(14, 17);
            this.maxStatusLabelX.Text = "Max";
            this.maxStatusLabelX.Visible = false;
            // 
            // autoScaleButtonX
            // 
            this.autoScaleButtonX.Checked = true;
            this.autoScaleButtonX.CheckOnClick = true;
            this.autoScaleButtonX.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoScaleButtonX.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.autoScaleButtonX.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.autoScaleButtonX.Name = "autoScaleButtonX";
            this.autoScaleButtonX.Size = new System.Drawing.Size(35, 20);
            this.autoScaleButtonX.Text = "auto";
            this.autoScaleButtonX.CheckedChanged += new System.EventHandler(this.autoScaleButtonX_CheckedChanged);
            // 
            // scaleStatusLabelY
            // 
            this.scaleStatusLabelY.Name = "statusLabelY";
            this.scaleStatusLabelY.Size = new System.Drawing.Size(47, 17);
            this.scaleStatusLabelY.Text = "Y:";
            // 
            // minStatusLabelY
            // 
            this.minStatusLabelY.Name = "minStatusLabelY";
            this.minStatusLabelY.Size = new System.Drawing.Size(13, 17);
            this.minStatusLabelY.Text = "min";
            this.minStatusLabelY.Visible = false;
            // 
            // maxStatusLabelY
            // 
            this.maxStatusLabelY.Name = "maxStatusLabelY";
            this.maxStatusLabelY.Size = new System.Drawing.Size(14, 17);
            this.maxStatusLabelY.Text = "Max";
            this.maxStatusLabelY.Visible = false;
            // 
            // autoScaleButtonY
            // 
            this.autoScaleButtonY.Checked = true;
            this.autoScaleButtonY.CheckOnClick = true;
            this.autoScaleButtonY.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoScaleButtonY.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.autoScaleButtonY.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.autoScaleButtonY.Name = "autoScaleButtonY";
            this.autoScaleButtonY.Size = new System.Drawing.Size(35, 20);
            this.autoScaleButtonY.Text = "auto";
            this.autoScaleButtonY.CheckedChanged += new System.EventHandler(this.autoScaleButtonY_CheckedChanged);
            // 
            // graph
            // 
            this.graph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graph.Location = new System.Drawing.Point(0, 0);
            this.graph.Name = "graph";
            this.graph.ScrollGrace = 0D;
            this.graph.ScrollMaxX = 0D;
            this.graph.ScrollMaxY = 0D;
            this.graph.ScrollMaxY2 = 0D;
            this.graph.ScrollMinX = 0D;
            this.graph.ScrollMinY = 0D;
            this.graph.ScrollMinY2 = 0D;
            this.graph.Size = new System.Drawing.Size(400, 218);
            this.graph.TabIndex = 2;
            this.graph.MouseMoveEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.graph_MouseMoveEvent);
            this.graph.MouseClick += new System.Windows.Forms.MouseEventHandler(this.graph_MouseClick);
            // 
            // LineGraphView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.graph);
            this.Controls.Add(this.statusStrip);
            this.Name = "LineGraphView";
            this.Size = new System.Drawing.Size(400, 240);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripButton autoScaleButtonX;
        private System.Windows.Forms.ToolStripButton autoScaleButtonY;
        private LineGraph graph;
        private System.Windows.Forms.ToolStripStatusLabel cursorStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel scaleStatusLabelX;
        private System.Windows.Forms.ToolStripStatusLabel minStatusLabelX;
        private System.Windows.Forms.ToolStripStatusLabel maxStatusLabelX;
        private System.Windows.Forms.ToolStripStatusLabel scaleStatusLabelY;
        private System.Windows.Forms.ToolStripStatusLabel minStatusLabelY;
        private System.Windows.Forms.ToolStripStatusLabel maxStatusLabelY;
        private System.Windows.Forms.ToolStripStatusLabel capacityStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel capacityValueLabel;
    }
}
