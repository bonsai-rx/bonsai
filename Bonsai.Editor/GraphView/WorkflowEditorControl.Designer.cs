namespace Bonsai.Editor.GraphView
{
    partial class WorkflowEditorControl
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
            this.tabContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tabControl = new Bonsai.Editor.GraphView.WorkflowEditorTabControl();
            this.workflowTabPage = new System.Windows.Forms.TabPage();
            this.browserLayoutPanel = new Bonsai.Editor.TableLayoutPanel();
            this.annotationPanel = new Bonsai.Editor.GraphView.AnnotationPanel();
            this.browserTitlePanel = new System.Windows.Forms.Panel();
            this.closeBrowserButton = new System.Windows.Forms.Button();
            this.browserLabel = new Bonsai.Editor.Label();
            this.tabContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.browserLayoutPanel.SuspendLayout();
            this.browserTitlePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabContextMenuStrip
            // 
            this.tabContextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.tabContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem,
            this.closeAllToolStripMenuItem});
            this.tabContextMenuStrip.Name = "tabContextMenuStrip";
            this.tabContextMenuStrip.Size = new System.Drawing.Size(150, 48);
            this.tabContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.tabContextMenuStrip_Opening);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F4)));
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // closeAllToolStripMenuItem
            // 
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.closeAllToolStripMenuItem.Text = "Close All";
            this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Vertical;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.browserLayoutPanel);
            this.splitContainer.Panel1Collapsed = true;
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.tabControl);
            this.splitContainer.Size = new System.Drawing.Size(300, 200);
            this.splitContainer.SplitterDistance = 300;
            this.splitContainer.SplitterWidth = 3;
            this.splitContainer.TabIndex = 1;
            this.splitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer_SplitterMoved);
            // 
            // tabControl
            // 
            this.tabControl.AdjustRectangle = new System.Windows.Forms.Padding(0);
            this.tabControl.Controls.Add(this.workflowTabPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(9, 6);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(300, 200);
            this.tabControl.TabIndex = 0;
            this.tabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl_Selecting);
            this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl_Selected);
            this.tabControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tabControl_KeyDown);
            this.tabControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tabControl_MouseUp);
            // 
            // workflowTabPage
            // 
            this.workflowTabPage.Location = new System.Drawing.Point(4, 28);
            this.workflowTabPage.Name = "workflowTabPage";
            this.workflowTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.workflowTabPage.Size = new System.Drawing.Size(292, 68);
            this.workflowTabPage.TabIndex = 0;
            this.workflowTabPage.Text = "Workflow";
            this.workflowTabPage.UseVisualStyleBackColor = true;
            // 
            // browserLayoutPanel
            // 
            this.browserLayoutPanel.ColumnCount = 1;
            this.browserLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.browserLayoutPanel.Controls.Add(this.annotationPanel, 0, 1);
            this.browserLayoutPanel.Controls.Add(this.browserTitlePanel, 0, 0);
            this.browserLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browserLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.browserLayoutPanel.Name = "browserLayoutPanel";
            this.browserLayoutPanel.RowCount = 2;
            this.browserLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.browserLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.browserLayoutPanel.Size = new System.Drawing.Size(300, 97);
            this.browserLayoutPanel.TabIndex = 1;
            // 
            // webView
            // 
            this.annotationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.annotationPanel.Location = new System.Drawing.Point(2, 25);
            this.annotationPanel.Margin = new System.Windows.Forms.Padding(2);
            this.annotationPanel.Name = "annotationPanel";
            this.annotationPanel.Size = new System.Drawing.Size(296, 70);
            this.annotationPanel.TabIndex = 0;
            this.annotationPanel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.annotationPanel_KeyDown);
            // 
            // browserTitlePanel
            // 
            this.browserTitlePanel.Controls.Add(this.closeBrowserButton);
            this.browserTitlePanel.Controls.Add(this.browserLabel);
            this.browserTitlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browserTitlePanel.Location = new System.Drawing.Point(0, 0);
            this.browserTitlePanel.Margin = new System.Windows.Forms.Padding(0);
            this.browserTitlePanel.Name = "browserTitlePanel";
            this.browserTitlePanel.Size = new System.Drawing.Size(300, 23);
            this.browserTitlePanel.TabIndex = 1;
            // 
            // closeBrowserButton
            // 
            this.closeBrowserButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.closeBrowserButton.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.closeBrowserButton.FlatAppearance.BorderSize = 0;
            this.closeBrowserButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeBrowserButton.Location = new System.Drawing.Point(271, 0);
            this.closeBrowserButton.Name = "closeBrowserButton";
            this.closeBrowserButton.Size = new System.Drawing.Size(25, 23);
            this.closeBrowserButton.TabIndex = 5;
            this.closeBrowserButton.Text = "✕";
            this.closeBrowserButton.UseVisualStyleBackColor = false;
            this.closeBrowserButton.Click += new System.EventHandler(this.closeBrowserButton_Click);
            // 
            // browserLabel
            // 
            this.browserLabel.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.browserLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browserLabel.Location = new System.Drawing.Point(0, 0);
            this.browserLabel.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.browserLabel.Name = "browserLabel";
            this.browserLabel.Size = new System.Drawing.Size(300, 23);
            this.browserLabel.TabIndex = 4;
            this.browserLabel.Text = "Browser";
            this.browserLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // WorkflowEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "WorkflowEditorControl";
            this.Size = new System.Drawing.Size(300, 200);
            this.tabContextMenuStrip.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.browserLayoutPanel.ResumeLayout(false);
            this.browserTitlePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Bonsai.Editor.GraphView.WorkflowEditorTabControl tabControl;
        private System.Windows.Forms.TabPage workflowTabPage;
        private System.Windows.Forms.ContextMenuStrip tabContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer;
        private Bonsai.Editor.GraphView.AnnotationPanel annotationPanel;
        private Bonsai.Editor.TableLayoutPanel browserLayoutPanel;
        private System.Windows.Forms.Panel browserTitlePanel;
        private System.Windows.Forms.Button closeBrowserButton;
        private Bonsai.Editor.Label browserLabel;
    }
}
