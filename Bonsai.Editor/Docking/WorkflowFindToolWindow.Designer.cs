namespace Bonsai.Editor.Docking
{
    partial class WorkflowFindToolWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.findListView = new Bonsai.Editor.ListView();
            this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pathColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operatorTypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.observableTypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openNewTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openNewWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // findListView
            // 
            this.findListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.findListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumnHeader,
            this.pathColumnHeader,
            this.operatorTypeColumnHeader,
            this.observableTypeColumnHeader});
            this.findListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.findListView.FullRowSelect = true;
            this.findListView.HideSelection = false;
            this.findListView.Location = new System.Drawing.Point(0, 0);
            this.findListView.MultiSelect = false;
            this.findListView.Name = "findListView";
            this.findListView.Size = new System.Drawing.Size(568, 261);
            this.findListView.TabIndex = 0;
            this.findListView.UseCompatibleStateImageBehavior = false;
            this.findListView.View = System.Windows.Forms.View.Details;
            this.findListView.ItemActivate += new System.EventHandler(this.findListView_ItemActivate);
            this.findListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.findListView_KeyDown);
            this.findListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.findListView_MouseClick);
            // 
            // nameColumnHeader
            // 
            this.nameColumnHeader.Text = "Name";
            this.nameColumnHeader.Width = 40;
            // 
            // pathColumnHeader
            // 
            this.pathColumnHeader.Text = "Workflow Path";
            this.pathColumnHeader.Width = 82;
            // 
            // operatorTypeColumnHeader
            // 
            this.operatorTypeColumnHeader.Text = "Operator Type";
            this.operatorTypeColumnHeader.Width = 80;
            // 
            // observableTypeColumnHeader
            // 
            this.observableTypeColumnHeader.Text = "Observable Type";
            this.observableTypeColumnHeader.Width = 362;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem,
            this.openNewTabToolStripMenuItem,
            this.openNewWindowToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(234, 70);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.ShortcutKeyDisplayString = "Enter";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.findListView_ItemActivate);
            // 
            // openNewTabToolStripMenuItem
            // 
            this.openNewTabToolStripMenuItem.Name = "openNewTabToolStripMenuItem";
            this.openNewTabToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.openNewTabToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.openNewTabToolStripMenuItem.Text = "Show in New Tab";
            this.openNewTabToolStripMenuItem.Click += new System.EventHandler(this.openNewTabToolStripMenuItem_Click);
            // 
            // openNewWindowToolStripMenuItem
            // 
            this.openNewWindowToolStripMenuItem.Name = "openNewWindowToolStripMenuItem";
            this.openNewWindowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.openNewWindowToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.openNewWindowToolStripMenuItem.Text = "Show in New Window";
            this.openNewWindowToolStripMenuItem.Click += new System.EventHandler(this.openNewWindowToolStripMenuItem_Click);
            // 
            // WorkflowFindToolWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 261);
            this.Controls.Add(this.findListView);
            this.Icon = global::Bonsai.Editor.Properties.Resources.Icon;
            this.Name = "WorkflowFindToolWindow";
            this.Text = "Find";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Bonsai.Editor.ListView findListView;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ColumnHeader pathColumnHeader;
        private System.Windows.Forms.ColumnHeader operatorTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader observableTypeColumnHeader;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem openNewTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openNewWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
    }
}
