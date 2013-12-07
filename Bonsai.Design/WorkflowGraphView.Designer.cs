namespace Bonsai.Design
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
            this.graphView = new Bonsai.Design.GraphView();
            this.SuspendLayout();
            // 
            // graphView
            // 
            this.graphView.AllowDrop = true;
            this.graphView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphView.Location = new System.Drawing.Point(0, 0);
            this.graphView.Name = "graphView";
            this.graphView.Nodes = null;
            this.graphView.SelectedNode = null;
            this.graphView.Size = new System.Drawing.Size(150, 150);
            this.graphView.TabIndex = 0;
            this.graphView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.graphView_ItemDrag);
            this.graphView.NodeMouseDoubleClick += new System.EventHandler<Bonsai.Design.GraphNodeMouseEventArgs>(this.graphView_NodeMouseDoubleClick);
            this.graphView.SelectedNodeChanged += new System.EventHandler(this.graphView_SelectedNodeChanged);
            this.graphView.DragDrop += new System.Windows.Forms.DragEventHandler(this.graphView_DragDrop);
            this.graphView.DragEnter += new System.Windows.Forms.DragEventHandler(this.graphView_DragEnter);
            this.graphView.DragOver += new System.Windows.Forms.DragEventHandler(this.graphView_DragOver);
            this.graphView.DragLeave += new System.EventHandler(this.graphView_DragLeave);
            this.graphView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.graphView_KeyDown);
            // 
            // WorkflowGraphView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.graphView);
            this.Name = "WorkflowGraphView";
            this.ResumeLayout(false);

        }

        #endregion

        private GraphView graphView;
    }
}
