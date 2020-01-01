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
            this.tabControl = new Bonsai.Editor.GraphView.WorkflowEditorTabControl();
            this.workflowTabPage = new System.Windows.Forms.TabPage();
            this.tabContextMenuStrip.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabContextMenuStrip
            // 
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
            // tabControl
            // 
            this.tabControl.Controls.Add(this.workflowTabPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(9, 6);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(300, 200);
            this.tabControl.TabIndex = 0;
            this.tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl_Selected);
            this.tabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl_Selecting);
            this.tabControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tabControl_KeyDown);
            this.tabControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tabControl_MouseUp);
            // 
            // workflowTabPage
            // 
            this.workflowTabPage.Location = new System.Drawing.Point(-1, 24);
            this.workflowTabPage.Name = "workflowTabPage";
            this.workflowTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.workflowTabPage.Size = new System.Drawing.Size(301, 176);
            this.workflowTabPage.TabIndex = 0;
            this.workflowTabPage.Text = "Workflow";
            this.workflowTabPage.UseVisualStyleBackColor = true;
            // 
            // WorkflowEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new System.Drawing.Size(250, 125);
            this.Name = "WorkflowEditorControl";
            this.Size = new System.Drawing.Size(300, 200);
            this.tabContextMenuStrip.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Bonsai.Editor.GraphView.WorkflowEditorTabControl tabControl;
        private System.Windows.Forms.TabPage workflowTabPage;
        private System.Windows.Forms.ContextMenuStrip tabContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
    }
}
