namespace Bonsai.Editor.GraphView
{
    partial class WorkflowGraphView
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorkflowGraphView));
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.outputToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.subjectTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.externalizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createPropertySourceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.visualizerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAsWorkflowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.groupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ungroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.enableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.graphView = new Bonsai.Editor.GraphView.GraphViewControl();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.outputToolStripMenuItem,
            this.subjectTypeToolStripMenuItem,
            this.externalizeToolStripMenuItem,
            this.createPropertySourceToolStripMenuItem,
            this.toolStripSeparator3,
            this.visualizerToolStripMenuItem,
            this.defaultEditorToolStripMenuItem,
            this.toolStripSeparator4,
            this.saveAsWorkflowToolStripMenuItem,
            this.toolStripSeparator2,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator1,
            this.connectToolStripMenuItem,
            this.disconnectToolStripMenuItem,
            this.reconnectToolStripMenuItem,
            this.toolStripSeparator5,
            this.groupToolStripMenuItem,
            this.ungroupToolStripMenuItem,
            this.toolStripSeparator6,
            this.enableToolStripMenuItem,
            this.disableToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(250, 458);
            this.contextMenuStrip.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.contextMenuStrip_Closed);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // outputToolStripMenuItem
            // 
            this.outputToolStripMenuItem.Enabled = false;
            this.outputToolStripMenuItem.Name = "outputToolStripMenuItem";
            this.outputToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.outputToolStripMenuItem.Text = "Output";
            // 
            // subjectTypeToolStripMenuItem
            // 
            this.subjectTypeToolStripMenuItem.Enabled = false;
            this.subjectTypeToolStripMenuItem.Name = "subjectTypeToolStripMenuItem";
            this.subjectTypeToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.subjectTypeToolStripMenuItem.Text = "Subject Type";
            this.subjectTypeToolStripMenuItem.Visible = false;
            // 
            // externalizeToolStripMenuItem
            // 
            this.externalizeToolStripMenuItem.Enabled = false;
            this.externalizeToolStripMenuItem.Name = "externalizeToolStripMenuItem";
            this.externalizeToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.externalizeToolStripMenuItem.Text = "Externalize Property";
            // 
            // createPropertySourceToolStripMenuItem
            // 
            this.createPropertySourceToolStripMenuItem.Enabled = false;
            this.createPropertySourceToolStripMenuItem.Name = "createPropertySourceToolStripMenuItem";
            this.createPropertySourceToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.createPropertySourceToolStripMenuItem.Text = "Create Property Source";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(246, 6);
            // 
            // visualizerToolStripMenuItem
            // 
            this.visualizerToolStripMenuItem.Enabled = false;
            this.visualizerToolStripMenuItem.Name = "visualizerToolStripMenuItem";
            this.visualizerToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.visualizerToolStripMenuItem.Text = "Show Visualizer";
            // 
            // defaultEditorToolStripMenuItem
            // 
            this.defaultEditorToolStripMenuItem.Enabled = false;
            this.defaultEditorToolStripMenuItem.Name = "defaultEditorToolStripMenuItem";
            this.defaultEditorToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Enter";
            this.defaultEditorToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.defaultEditorToolStripMenuItem.Text = "Show Default Editor...";
            this.defaultEditorToolStripMenuItem.Click += new System.EventHandler(this.defaultEditorToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(246, 6);
            // 
            // saveAsWorkflowToolStripMenuItem
            // 
            this.saveAsWorkflowToolStripMenuItem.Enabled = false;
            this.saveAsWorkflowToolStripMenuItem.Name = "saveAsWorkflowToolStripMenuItem";
            this.saveAsWorkflowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsWorkflowToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.saveAsWorkflowToolStripMenuItem.Text = "Save As Workflow...";
            this.saveAsWorkflowToolStripMenuItem.Click += new System.EventHandler(this.saveAsWorkflowToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(246, 6);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Enabled = false;
            this.cutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem.Image")));
            this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Enabled = false;
            this.copyToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem.Image")));
            this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Enabled = false;
            this.pasteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem.Image")));
            this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Enabled = false;
            this.deleteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("deleteToolStripMenuItem.Image")));
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(246, 6);
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Enabled = false;
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.connectToolStripMenuItem.Text = "Create Connection";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Enabled = false;
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.disconnectToolStripMenuItem.Text = "Remove Connection";
            this.disconnectToolStripMenuItem.Click += new System.EventHandler(this.disconnectToolStripMenuItem_Click);
            // 
            // reconnectToolStripMenuItem
            // 
            this.reconnectToolStripMenuItem.Enabled = false;
            this.reconnectToolStripMenuItem.Name = "reconnectToolStripMenuItem";
            this.reconnectToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.reconnectToolStripMenuItem.Text = "Reorder Connection";
            this.reconnectToolStripMenuItem.Click += new System.EventHandler(this.reconnectToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(246, 6);
            // 
            // groupToolStripMenuItem
            // 
            this.groupToolStripMenuItem.Enabled = false;
            this.groupToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("groupToolStripMenuItem.Image")));
            this.groupToolStripMenuItem.Name = "groupToolStripMenuItem";
            this.groupToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.groupToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.groupToolStripMenuItem.Text = "Group";
            this.groupToolStripMenuItem.Click += new System.EventHandler(this.groupToolStripMenuItem_Click);
            // 
            // ungroupToolStripMenuItem
            // 
            this.ungroupToolStripMenuItem.Enabled = false;
            this.ungroupToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("ungroupToolStripMenuItem.Image")));
            this.ungroupToolStripMenuItem.Name = "ungroupToolStripMenuItem";
            this.ungroupToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.G)));
            this.ungroupToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.ungroupToolStripMenuItem.Text = "Ungroup";
            this.ungroupToolStripMenuItem.Click += new System.EventHandler(this.ungroupToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(246, 6);
            // 
            // enableToolStripMenuItem
            // 
            this.enableToolStripMenuItem.Enabled = false;
            this.enableToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("enableToolStripMenuItem.Image")));
            this.enableToolStripMenuItem.Name = "enableToolStripMenuItem";
            this.enableToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D)));
            this.enableToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.enableToolStripMenuItem.Text = "Enable";
            this.enableToolStripMenuItem.Click += new System.EventHandler(this.enableToolStripMenuItem_Click);
            // 
            // disableToolStripMenuItem
            // 
            this.disableToolStripMenuItem.Enabled = false;
            this.disableToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("disableToolStripMenuItem.Image")));
            this.disableToolStripMenuItem.Name = "disableToolStripMenuItem";
            this.disableToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.disableToolStripMenuItem.Size = new System.Drawing.Size(249, 22);
            this.disableToolStripMenuItem.Text = "Disable";
            this.disableToolStripMenuItem.Click += new System.EventHandler(this.disableToolStripMenuItem_Click);
            // 
            // graphView
            // 
            this.graphView.AllowDrop = true;
            this.graphView.BackColor = System.Drawing.Color.White;
            this.graphView.ContextMenuStrip = this.contextMenuStrip;
            this.graphView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphView.Location = new System.Drawing.Point(0, 0);
            this.graphView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.graphView.Name = "graphView";
            this.graphView.Nodes = null;
            this.graphView.Padding = new System.Windows.Forms.Padding(3);
            this.graphView.Size = new System.Drawing.Size(150, 150);
            this.graphView.TabIndex = 0;
            this.graphView.TextDrawMode = Bonsai.Editor.GraphView.GraphViewTextDrawMode.All;
            this.graphView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.graphView_ItemDrag);
            this.graphView.NodeMouseDoubleClick += new System.EventHandler<Bonsai.Editor.GraphView.GraphNodeMouseEventArgs>(this.graphView_NodeMouseDoubleClick);
            this.graphView.NodeMouseEnter += new System.EventHandler<Bonsai.Editor.GraphView.GraphNodeMouseEventArgs>(this.graphView_NodeMouseEnter);
            this.graphView.NodeMouseLeave += new System.EventHandler<Bonsai.Editor.GraphView.GraphNodeMouseEventArgs>(this.graphView_NodeMouseLeave);
            this.graphView.SelectedNodeChanged += new System.EventHandler(this.graphView_SelectedNodeChanged);
            this.graphView.DragDrop += new System.Windows.Forms.DragEventHandler(this.graphView_DragDrop);
            this.graphView.DragEnter += new System.Windows.Forms.DragEventHandler(this.graphView_DragEnter);
            this.graphView.DragOver += new System.Windows.Forms.DragEventHandler(this.graphView_DragOver);
            this.graphView.DragLeave += new System.EventHandler(this.graphView_DragLeave);
            this.graphView.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.graphView_GiveFeedback);
            this.graphView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.graphView_KeyDown);
            this.graphView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.graphView_KeyPress);
            this.graphView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphView_MouseDown);
            // 
            // WorkflowGraphView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.graphView);
            this.Name = "WorkflowGraphView";
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GraphViewControl graphView;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem groupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem visualizerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem outputToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem externalizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createPropertySourceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reconnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem saveAsWorkflowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ungroupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem subjectTypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem enableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disableToolStripMenuItem;
    }
}
