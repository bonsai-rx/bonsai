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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartScreen));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.recentLabel = new System.Windows.Forms.Label();
            this.openTreeView = new System.Windows.Forms.TreeView();
            this.openLabel = new System.Windows.Forms.Label();
            this.getStartedTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.getStartedLabel = new System.Windows.Forms.Label();
            this.getStartedTreeView = new System.Windows.Forms.TreeView();
            this.recentTreeView = new System.Windows.Forms.TreeView();
            this.openWorkflowDialog = new System.Windows.Forms.OpenFileDialog();
            this.logoPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel.SuspendLayout();
            this.getStartedTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Controls.Add(this.recentLabel, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.openTreeView, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.openLabel, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.getStartedTableLayoutPanel, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.recentTreeView, 1, 2);
            this.tableLayoutPanel.Controls.Add(this.logoPanel, 1, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 3;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 128F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 39F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(484, 361);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // recentLabel
            // 
            this.recentLabel.AutoSize = true;
            this.recentLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recentLabel.Font = new System.Drawing.Font("Calibri Light", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.recentLabel.ForeColor = System.Drawing.Color.SeaGreen;
            this.recentLabel.Location = new System.Drawing.Point(250, 128);
            this.recentLabel.Margin = new System.Windows.Forms.Padding(8, 0, 3, 0);
            this.recentLabel.Name = "recentLabel";
            this.recentLabel.Size = new System.Drawing.Size(231, 39);
            this.recentLabel.TabIndex = 5;
            this.recentLabel.Text = "Recent";
            this.recentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // openTreeView
            // 
            this.openTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.openTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.openTreeView.Font = new System.Drawing.Font("Calibri Light", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openTreeView.HotTracking = true;
            this.openTreeView.ItemHeight = 20;
            this.openTreeView.Location = new System.Drawing.Point(3, 170);
            this.openTreeView.Name = "openTreeView";
            this.openTreeView.ShowLines = false;
            this.openTreeView.Size = new System.Drawing.Size(236, 188);
            this.openTreeView.TabIndex = 1;
            this.openTreeView.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView_DrawNode);
            this.openTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeSelect);
            this.openTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            this.openTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyDown);
            // 
            // openLabel
            // 
            this.openLabel.AutoSize = true;
            this.openLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.openLabel.Font = new System.Drawing.Font("Calibri Light", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.openLabel.ForeColor = System.Drawing.Color.SeaGreen;
            this.openLabel.Location = new System.Drawing.Point(20, 128);
            this.openLabel.Margin = new System.Windows.Forms.Padding(20, 0, 3, 0);
            this.openLabel.Name = "openLabel";
            this.openLabel.Size = new System.Drawing.Size(219, 39);
            this.openLabel.TabIndex = 4;
            this.openLabel.Text = "Open";
            this.openLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // getStartedTableLayoutPanel
            // 
            this.getStartedTableLayoutPanel.ColumnCount = 1;
            this.getStartedTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.getStartedTableLayoutPanel.Controls.Add(this.getStartedLabel, 0, 0);
            this.getStartedTableLayoutPanel.Controls.Add(this.getStartedTreeView, 0, 1);
            this.getStartedTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.getStartedTableLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.getStartedTableLayoutPanel.Name = "getStartedTableLayoutPanel";
            this.getStartedTableLayoutPanel.RowCount = 2;
            this.getStartedTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.getStartedTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.getStartedTableLayoutPanel.Size = new System.Drawing.Size(236, 122);
            this.getStartedTableLayoutPanel.TabIndex = 7;
            // 
            // getStartedLabel
            // 
            this.getStartedLabel.AutoSize = true;
            this.getStartedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.getStartedLabel.Font = new System.Drawing.Font("Calibri Light", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.getStartedLabel.ForeColor = System.Drawing.Color.SeaGreen;
            this.getStartedLabel.Location = new System.Drawing.Point(16, 0);
            this.getStartedLabel.Margin = new System.Windows.Forms.Padding(16, 0, 3, 0);
            this.getStartedLabel.Name = "getStartedLabel";
            this.getStartedLabel.Size = new System.Drawing.Size(217, 46);
            this.getStartedLabel.TabIndex = 5;
            this.getStartedLabel.Text = "Get Started";
            this.getStartedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.getStartedTreeView.Size = new System.Drawing.Size(230, 70);
            this.getStartedTreeView.TabIndex = 0;
            this.getStartedTreeView.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView_DrawNode);
            this.getStartedTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeSelect);
            this.getStartedTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_NodeMouseClick);
            this.getStartedTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeView_KeyDown);
            // 
            // recentTreeView
            // 
            this.recentTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.recentTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recentTreeView.Location = new System.Drawing.Point(245, 170);
            this.recentTreeView.Name = "recentTreeView";
            this.recentTreeView.Size = new System.Drawing.Size(236, 188);
            this.recentTreeView.TabIndex = 2;
            // 
            // openWorkflowDialog
            // 
            this.openWorkflowDialog.Filter = "Bonsai Files|*.bonsai";
            // 
            // logoPanel
            // 
            this.logoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logoPanel.Location = new System.Drawing.Point(245, 3);
            this.logoPanel.Name = "logoPanel";
            this.logoPanel.Size = new System.Drawing.Size(236, 122);
            this.logoPanel.TabIndex = 8;
            // 
            // LandingPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.tableLayoutPanel);
            this.Icon = global::Bonsai.Editor.Properties.Resources.Icon;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(400, 300);
            this.Name = "LandingPage";
            this.Text = "Bonsai";
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.getStartedTableLayoutPanel.ResumeLayout(false);
            this.getStartedTableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.TreeView openTreeView;
        private System.Windows.Forms.Label openLabel;
        private System.Windows.Forms.Label recentLabel;
        private System.Windows.Forms.TableLayoutPanel getStartedTableLayoutPanel;
        private System.Windows.Forms.Label getStartedLabel;
        private System.Windows.Forms.TreeView getStartedTreeView;
        private System.Windows.Forms.OpenFileDialog openWorkflowDialog;
        private System.Windows.Forms.TreeView recentTreeView;
        private System.Windows.Forms.Panel logoPanel;
    }
}

