namespace Bonsai.Editor.Docking
{
    partial class WatchToolWindow
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
            this.watchListView = new Bonsai.Editor.ListView();
            this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pathColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.operatorTypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.observableTypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openNewTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openNewWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteWatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // watchListView
            // 
            this.watchListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.watchListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumnHeader,
            this.pathColumnHeader,
            this.operatorTypeColumnHeader,
            this.observableTypeColumnHeader});
            this.watchListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.watchListView.FullRowSelect = true;
            this.watchListView.HideSelection = false;
            this.watchListView.Location = new System.Drawing.Point(0, 0);
            this.watchListView.Name = "watchListView";
            this.watchListView.Size = new System.Drawing.Size(568, 261);
            this.watchListView.TabIndex = 0;
            this.watchListView.UseCompatibleStateImageBehavior = false;
            this.watchListView.View = System.Windows.Forms.View.Details;
            this.watchListView.ItemActivate += new System.EventHandler(this.watchListView_ItemActivate);
            this.watchListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.watchListView_MouseClick);
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
            this.openNewWindowToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteWatchToolStripMenuItem,
            this.selectAllToolStripMenuItem,
            this.clearAllToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(234, 164);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.ShortcutKeyDisplayString = "Enter";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.showToolStripMenuItem.Text = "Show";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.watchListView_ItemActivate);
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
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(230, 6);
            // 
            // deleteWatchToolStripMenuItem
            // 
            this.deleteWatchToolStripMenuItem.Image = global::Bonsai.Editor.Properties.Resources.DeleteWatchMenuImage;
            this.deleteWatchToolStripMenuItem.Name = "deleteWatchToolStripMenuItem";
            this.deleteWatchToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteWatchToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.deleteWatchToolStripMenuItem.Text = "Delete Watch";
            this.deleteWatchToolStripMenuItem.Click += new System.EventHandler(this.deleteWatchToolStripMenuItem_Click);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Image = global::Bonsai.Editor.Properties.Resources.SelectAllMenuImage;
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // clearAllToolStripMenuItem
            // 
            this.clearAllToolStripMenuItem.Image = global::Bonsai.Editor.Properties.Resources.ClearAllMenuImage;
            this.clearAllToolStripMenuItem.Name = "clearAllToolStripMenuItem";
            this.clearAllToolStripMenuItem.Size = new System.Drawing.Size(233, 22);
            this.clearAllToolStripMenuItem.Text = "Clear All";
            this.clearAllToolStripMenuItem.Click += new System.EventHandler(this.clearAllToolStripMenuItem_Click);
            // 
            // WatchToolWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 261);
            this.Controls.Add(this.watchListView);
            this.Icon = global::Bonsai.Editor.Properties.Resources.Icon;
            this.Name = "WatchToolWindow";
            this.Text = "Watch";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Bonsai.Editor.ListView watchListView;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ColumnHeader pathColumnHeader;
        private System.Windows.Forms.ColumnHeader operatorTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader observableTypeColumnHeader;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem openNewTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openNewWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteWatchToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearAllToolStripMenuItem;
    }
}
