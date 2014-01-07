namespace Bonsai.Dsp.Design
{
    partial class WaveformGraph
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
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.xminStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.xmaxStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.autoScaleXButton = new System.Windows.Forms.ToolStripButton();
            this.scaleStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.yminStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ymaxStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.autoScaleYButton = new System.Windows.Forms.ToolStripButton();
            this.chart = new Bonsai.Design.Visualizers.ChartControl();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cursorStatusLabel,
            this.toolStripStatusLabel1,
            this.xminStatusLabel,
            this.xmaxStatusLabel,
            this.autoScaleXButton,
            this.scaleStatusLabel,
            this.yminStatusLabel,
            this.ymaxStatusLabel,
            this.autoScaleYButton});
            this.statusStrip.Location = new System.Drawing.Point(0, 218);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(320, 22);
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
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(44, 17);
            this.toolStripStatusLabel1.Text = "ScaleX:";
            // 
            // xminStatusLabel
            // 
            this.xminStatusLabel.DoubleClickEnabled = true;
            this.xminStatusLabel.Name = "xminStatusLabel";
            this.xminStatusLabel.Size = new System.Drawing.Size(12, 17);
            this.xminStatusLabel.Text = "x";
            this.xminStatusLabel.Visible = false;
            this.xminStatusLabel.DoubleClick += new System.EventHandler(this.editableStatusLabel_DoubleClick);
            // 
            // xmaxStatusLabel
            // 
            this.xmaxStatusLabel.DoubleClickEnabled = true;
            this.xmaxStatusLabel.Name = "xmaxStatusLabel";
            this.xmaxStatusLabel.Size = new System.Drawing.Size(14, 17);
            this.xmaxStatusLabel.Text = "X";
            this.xmaxStatusLabel.Visible = false;
            this.xmaxStatusLabel.DoubleClick += new System.EventHandler(this.editableStatusLabel_DoubleClick);
            // 
            // autoScaleXButton
            // 
            this.autoScaleXButton.Checked = true;
            this.autoScaleXButton.CheckOnClick = true;
            this.autoScaleXButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoScaleXButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.autoScaleXButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.autoScaleXButton.Name = "autoScaleXButton";
            this.autoScaleXButton.Size = new System.Drawing.Size(35, 20);
            this.autoScaleXButton.Text = "auto";
            this.autoScaleXButton.CheckedChanged += new System.EventHandler(this.autoScaleXButton_CheckedChanged);
            // 
            // scaleStatusLabel
            // 
            this.scaleStatusLabel.Name = "scaleStatusLabel";
            this.scaleStatusLabel.Size = new System.Drawing.Size(44, 17);
            this.scaleStatusLabel.Text = "ScaleY:";
            // 
            // yminStatusLabel
            // 
            this.yminStatusLabel.DoubleClickEnabled = true;
            this.yminStatusLabel.Name = "yminStatusLabel";
            this.yminStatusLabel.Size = new System.Drawing.Size(13, 17);
            this.yminStatusLabel.Text = "y";
            this.yminStatusLabel.Visible = false;
            this.yminStatusLabel.DoubleClick += new System.EventHandler(this.editableStatusLabel_DoubleClick);
            // 
            // ymaxStatusLabel
            // 
            this.ymaxStatusLabel.DoubleClickEnabled = true;
            this.ymaxStatusLabel.Name = "ymaxStatusLabel";
            this.ymaxStatusLabel.Size = new System.Drawing.Size(14, 17);
            this.ymaxStatusLabel.Text = "Y";
            this.ymaxStatusLabel.Visible = false;
            this.ymaxStatusLabel.DoubleClick += new System.EventHandler(this.editableStatusLabel_DoubleClick);
            // 
            // autoScaleYButton
            // 
            this.autoScaleYButton.Checked = true;
            this.autoScaleYButton.CheckOnClick = true;
            this.autoScaleYButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoScaleYButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.autoScaleYButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.autoScaleYButton.Name = "autoScaleYButton";
            this.autoScaleYButton.Size = new System.Drawing.Size(35, 20);
            this.autoScaleYButton.Text = "auto";
            this.autoScaleYButton.CheckedChanged += new System.EventHandler(this.autoScaleYButton_CheckedChanged);
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
            this.chart.Size = new System.Drawing.Size(320, 218);
            this.chart.TabIndex = 2;
            this.chart.ZoomEvent += new ZedGraph.ZedGraphControl.ZoomEventHandler(this.chart_ZoomEvent);
            this.chart.MouseMoveEvent += new ZedGraph.ZedGraphControl.ZedMouseEventHandler(this.chart_MouseMoveEvent);
            this.chart.MouseClick += new System.Windows.Forms.MouseEventHandler(this.chart_MouseClick);
            // 
            // WaveformGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chart);
            this.Controls.Add(this.statusStrip);
            this.Name = "WaveformGraph";
            this.Size = new System.Drawing.Size(320, 240);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripButton autoScaleYButton;
        private Bonsai.Design.Visualizers.ChartControl chart;
        private System.Windows.Forms.ToolStripStatusLabel cursorStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel scaleStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel yminStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel ymaxStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel xminStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel xmaxStatusLabel;
        private System.Windows.Forms.ToolStripButton autoScaleXButton;
    }
}
