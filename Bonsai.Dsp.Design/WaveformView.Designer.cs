namespace Bonsai.Dsp.Design
{
    partial class WaveformView
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
            this.scaleXStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.xminStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.xmaxStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.autoScaleXButton = new System.Windows.Forms.ToolStripButton();
            this.scaleYStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.yminStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ymaxStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.autoScaleYButton = new System.Windows.Forms.ToolStripButton();
            this.overlayModeSplitButton = new System.Windows.Forms.ToolStripSplitButton();
            this.chart = new Bonsai.Dsp.Design.WaveformGraph();
            this.historyLengthNumericUpDown = new Bonsai.Dsp.Design.ToolStripLabeledNumericUpDown();
            this.bufferLengthNumericUpDown = new Bonsai.Dsp.Design.ToolStripLabeledNumericUpDown();
            this.channelOffsetNumericUpDown = new Bonsai.Dsp.Design.ToolStripLabeledNumericUpDown();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cursorStatusLabel,
            this.scaleXStatusLabel,
            this.xminStatusLabel,
            this.xmaxStatusLabel,
            this.autoScaleXButton,
            this.scaleYStatusLabel,
            this.yminStatusLabel,
            this.ymaxStatusLabel,
            this.autoScaleYButton,
            this.overlayModeSplitButton});
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
            // scaleXStatusLabel
            // 
            this.scaleXStatusLabel.Name = "scaleXStatusLabel";
            this.scaleXStatusLabel.Size = new System.Drawing.Size(47, 17);
            this.scaleXStatusLabel.Text = "Scale X:";
            // 
            // xminStatusLabel
            // 
            this.xminStatusLabel.Name = "xminStatusLabel";
            this.xminStatusLabel.Size = new System.Drawing.Size(12, 17);
            this.xminStatusLabel.Text = "x";
            this.xminStatusLabel.Visible = false;
            this.xminStatusLabel.Click += new System.EventHandler(this.editableStatusLabel_Click);
            // 
            // xmaxStatusLabel
            // 
            this.xmaxStatusLabel.Name = "xmaxStatusLabel";
            this.xmaxStatusLabel.Size = new System.Drawing.Size(14, 17);
            this.xmaxStatusLabel.Text = "X";
            this.xmaxStatusLabel.Visible = false;
            this.xmaxStatusLabel.Click += new System.EventHandler(this.editableStatusLabel_Click);
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
            // scaleYStatusLabel
            // 
            this.scaleYStatusLabel.Name = "scaleYStatusLabel";
            this.scaleYStatusLabel.Size = new System.Drawing.Size(47, 17);
            this.scaleYStatusLabel.Text = "Scale Y:";
            // 
            // yminStatusLabel
            // 
            this.yminStatusLabel.Name = "yminStatusLabel";
            this.yminStatusLabel.Size = new System.Drawing.Size(13, 17);
            this.yminStatusLabel.Text = "y";
            this.yminStatusLabel.Visible = false;
            this.yminStatusLabel.Click += new System.EventHandler(this.editableStatusLabel_Click);
            // 
            // ymaxStatusLabel
            // 
            this.ymaxStatusLabel.Name = "ymaxStatusLabel";
            this.ymaxStatusLabel.Size = new System.Drawing.Size(14, 17);
            this.ymaxStatusLabel.Text = "Y";
            this.ymaxStatusLabel.Visible = false;
            this.ymaxStatusLabel.Click += new System.EventHandler(this.editableStatusLabel_Click);
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
            // overlayModeSplitButton
            // 
            this.overlayModeSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.overlayModeSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.historyLengthNumericUpDown,
            this.bufferLengthNumericUpDown,
            this.channelOffsetNumericUpDown});
            this.overlayModeSplitButton.Image = global::Bonsai.Dsp.Design.Properties.Resources.OverlayGridModeImage;
            this.overlayModeSplitButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.overlayModeSplitButton.Name = "overlayModeSplitButton";
            this.overlayModeSplitButton.Size = new System.Drawing.Size(32, 20);
            this.overlayModeSplitButton.ButtonClick += new System.EventHandler(this.overlayModeSplitButton_Click);
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
            // historyLengthNumericUpDown
            // 
            this.historyLengthNumericUpDown.DecimalPlaces = 0;
            this.historyLengthNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.historyLengthNumericUpDown.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.historyLengthNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.historyLengthNumericUpDown.Name = "historyLengthNumericUpDown";
            this.historyLengthNumericUpDown.Size = new System.Drawing.Size(143, 29);
            this.historyLengthNumericUpDown.Text = "History Length";
            this.historyLengthNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.historyLengthNumericUpDown.ValueChanged += new System.EventHandler(this.historyLengthNumericUpDown_ValueChanged);
            // 
            // bufferLengthNumericUpDown
            // 
            this.bufferLengthNumericUpDown.DecimalPlaces = 0;
            this.bufferLengthNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.bufferLengthNumericUpDown.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.bufferLengthNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.bufferLengthNumericUpDown.Name = "bufferLengthNumericUpDown";
            this.bufferLengthNumericUpDown.Size = new System.Drawing.Size(151, 29);
            this.bufferLengthNumericUpDown.Text = "Display Previous";
            this.bufferLengthNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.bufferLengthNumericUpDown.ValueChanged += new System.EventHandler(this.bufferLengthNumericUpDown_ValueChanged);
            // 
            // channelOffsetNumericUpDown
            // 
            this.channelOffsetNumericUpDown.DecimalPlaces = 0;
            this.channelOffsetNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.channelOffsetNumericUpDown.Maximum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.channelOffsetNumericUpDown.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.channelOffsetNumericUpDown.Name = "channelOffsetNumericUpDown";
            this.channelOffsetNumericUpDown.Size = new System.Drawing.Size(144, 29);
            this.channelOffsetNumericUpDown.Text = "Channel Offset";
            this.channelOffsetNumericUpDown.Value = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.channelOffsetNumericUpDown.ValueChanged += new System.EventHandler(this.channelOffsetNumericUpDown_ValueChanged);
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
        private System.Windows.Forms.ToolStripButton autoScaleYButton;
        private WaveformGraph chart;
        private System.Windows.Forms.ToolStripStatusLabel cursorStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel scaleYStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel yminStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel ymaxStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel scaleXStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel xminStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel xmaxStatusLabel;
        private System.Windows.Forms.ToolStripButton autoScaleXButton;
        private System.Windows.Forms.ToolStripSplitButton overlayModeSplitButton;
        private ToolStripLabeledNumericUpDown channelOffsetNumericUpDown;
        private ToolStripLabeledNumericUpDown bufferLengthNumericUpDown;
        private ToolStripLabeledNumericUpDown historyLengthNumericUpDown;
    }
}
