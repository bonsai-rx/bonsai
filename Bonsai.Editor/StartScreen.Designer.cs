namespace Bonsai.Editor
{
    partial class StartScreen
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
                DisposeDrawResources();
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
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.startLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.openTreeView = new System.Windows.Forms.TreeView();
            this.iconList = new System.Windows.Forms.ImageList(this.components);
            this.getStartedLabel = new System.Windows.Forms.Label();
            this.openLabel = new System.Windows.Forms.Label();
            this.getStartedTreeView = new System.Windows.Forms.TreeView();
            this.recentLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.recentLabel = new System.Windows.Forms.Label();
            this.openWorkflowDialog = new System.Windows.Forms.OpenFileDialog();
            this.recentFileView = new Bonsai.Editor.RecentlyUsedFileView();
            this.tableLayoutPanel.SuspendLayout();
            this.startLayoutPanel.SuspendLayout();
            this.recentLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 242F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.startLayoutPanel, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.recentLayoutPanel, 1, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 1;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 361F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(484, 361);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // startLayoutPanel
            // 
            this.startLayoutPanel.ColumnCount = 1;
            this.startLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.startLayoutPanel.Controls.Add(this.openTreeView, 0, 3);
            this.startLayoutPanel.Controls.Add(this.getStartedLabel, 0, 0);
            this.startLayoutPanel.Controls.Add(this.openLabel, 0, 2);
            this.startLayoutPanel.Controls.Add(this.getStartedTreeView, 0, 1);
            this.startLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.startLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.startLayoutPanel.Name = "startLayoutPanel";
            this.startLayoutPanel.RowCount = 4;
            this.startLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.startLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 76F));
            this.startLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.startLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 187F));
            this.startLayoutPanel.Size = new System.Drawing.Size(236, 355);
            this.startLayoutPanel.TabIndex = 7;
            // 
            // openTreeView
            // 
            this.openTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.openTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.openTreeView.Font = new System.Drawing.Font("Calibri Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openTreeView.HotTracking = true;
            this.openTreeView.ImageIndex = 0;
            this.openTreeView.ImageList = this.iconList;
            this.openTreeView.ItemHeight = 20;
            this.openTreeView.Location = new System.Drawing.Point(3, 171);
            this.openTreeView.Name = "openTreeView";
            this.openTreeView.SelectedImageIndex = 0;
            this.openTreeView.ShowLines = false;
            this.openTreeView.ShowRootLines = false;
            this.openTreeView.Size = new System.Drawing.Size(230, 181);
            this.openTreeView.TabIndex = 1;
            this.openTreeView.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView_DrawNode);
            this.openTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeSelect);
            this.openTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            this.openTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyDown);
            // 
            // iconList
            // 
            this.iconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.iconList.ImageSize = new System.Drawing.Size(16, 16);
            this.iconList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // getStartedLabel
            // 
            this.getStartedLabel.AutoSize = true;
            this.getStartedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.getStartedLabel.Font = new System.Drawing.Font("Calibri Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getStartedLabel.ForeColor = System.Drawing.Color.SeaGreen;
            this.getStartedLabel.Location = new System.Drawing.Point(8, 0);
            this.getStartedLabel.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
            this.getStartedLabel.Name = "getStartedLabel";
            this.getStartedLabel.Size = new System.Drawing.Size(225, 46);
            this.getStartedLabel.TabIndex = 5;
            this.getStartedLabel.Text = "Get Started";
            this.getStartedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openLabel
            // 
            this.openLabel.AutoSize = true;
            this.openLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openLabel.Font = new System.Drawing.Font("Calibri Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openLabel.ForeColor = System.Drawing.Color.SeaGreen;
            this.openLabel.Location = new System.Drawing.Point(8, 122);
            this.openLabel.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
            this.openLabel.Name = "openLabel";
            this.openLabel.Size = new System.Drawing.Size(225, 46);
            this.openLabel.TabIndex = 4;
            this.openLabel.Text = "Open";
            this.openLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // getStartedTreeView
            // 
            this.getStartedTreeView.BackColor = System.Drawing.SystemColors.Window;
            this.getStartedTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.getStartedTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.getStartedTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.getStartedTreeView.Font = new System.Drawing.Font("Calibri Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getStartedTreeView.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.getStartedTreeView.HotTracking = true;
            this.getStartedTreeView.ItemHeight = 20;
            this.getStartedTreeView.Location = new System.Drawing.Point(3, 49);
            this.getStartedTreeView.Name = "getStartedTreeView";
            this.getStartedTreeView.ShowLines = false;
            this.getStartedTreeView.ShowRootLines = false;
            this.getStartedTreeView.Size = new System.Drawing.Size(230, 70);
            this.getStartedTreeView.TabIndex = 0;
            this.getStartedTreeView.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView_DrawNode);
            this.getStartedTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeSelect);
            this.getStartedTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            this.getStartedTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyDown);
            // 
            // recentLayoutPanel
            // 
            this.recentLayoutPanel.ColumnCount = 1;
            this.recentLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.recentLayoutPanel.Controls.Add(this.recentLabel, 0, 0);
            this.recentLayoutPanel.Controls.Add(this.recentFileView, 0, 1);
            this.recentLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recentLayoutPanel.Location = new System.Drawing.Point(245, 3);
            this.recentLayoutPanel.Name = "recentLayoutPanel";
            this.recentLayoutPanel.RowCount = 2;
            this.recentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.recentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.recentLayoutPanel.Size = new System.Drawing.Size(236, 355);
            this.recentLayoutPanel.TabIndex = 8;
            // 
            // recentLabel
            // 
            this.recentLabel.AutoSize = true;
            this.recentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recentLabel.Font = new System.Drawing.Font("Calibri Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.recentLabel.ForeColor = System.Drawing.Color.SeaGreen;
            this.recentLabel.Location = new System.Drawing.Point(1, 0);
            this.recentLabel.Margin = new System.Windows.Forms.Padding(1, 0, 3, 0);
            this.recentLabel.Name = "recentLabel";
            this.recentLabel.Size = new System.Drawing.Size(232, 46);
            this.recentLabel.TabIndex = 5;
            this.recentLabel.Text = "Recent";
            this.recentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openWorkflowDialog
            // 
            this.openWorkflowDialog.Filter = "Bonsai Files|*.bonsai";
            // 
            // recentFileView
            // 
            this.recentFileView.BackColor = System.Drawing.SystemColors.Window;
            this.recentFileView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.recentFileView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recentFileView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.recentFileView.Font = new System.Drawing.Font("Calibri Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.recentFileView.FullRowSelect = true;
            this.recentFileView.HotTracking = true;
            this.recentFileView.ItemHeight = 50;
            this.recentFileView.LineColor = System.Drawing.Color.FromArgb(130, 183, 223);
            this.recentFileView.Location = new System.Drawing.Point(3, 49);
            this.recentFileView.Name = "recentFileView";
            this.recentFileView.ShowLines = false;
            this.recentFileView.ShowRootLines = false;
            this.recentFileView.Size = new System.Drawing.Size(230, 303);
            this.recentFileView.TabIndex = 2;
            this.recentFileView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeSelect);
            this.recentFileView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            this.recentFileView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyDown);
            // 
            // StartScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.tableLayoutPanel);
            this.Icon = global::Bonsai.Editor.Properties.Resources.Icon;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "StartScreen";
            this.Text = "Bonsai";
            this.tableLayoutPanel.ResumeLayout(false);
            this.startLayoutPanel.ResumeLayout(false);
            this.startLayoutPanel.PerformLayout();
            this.recentLayoutPanel.ResumeLayout(false);
            this.recentLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.TreeView openTreeView;
        private System.Windows.Forms.Label openLabel;
        private System.Windows.Forms.Label recentLabel;
        private System.Windows.Forms.TableLayoutPanel startLayoutPanel;
        private System.Windows.Forms.Label getStartedLabel;
        private System.Windows.Forms.TreeView getStartedTreeView;
        private System.Windows.Forms.OpenFileDialog openWorkflowDialog;
        private Bonsai.Editor.RecentlyUsedFileView recentFileView;
        private System.Windows.Forms.TableLayoutPanel recentLayoutPanel;
        private System.Windows.Forms.ImageList iconList;
    }
}

