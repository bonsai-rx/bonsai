namespace Bonsai.NuGet
{
    partial class PackageOperationDialog
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
            ClearEventLogger();
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
            this.actionNamePanel = new System.Windows.Forms.Panel();
            this.actionNameLabel = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.loggerListBox = new System.Windows.Forms.ListBox();
            this.closeButtonLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.closeButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.actionNamePanel.SuspendLayout();
            this.closeButtonLayoutPanel.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // actionNamePanel
            // 
            this.actionNamePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.actionNamePanel.Controls.Add(this.actionNameLabel);
            this.actionNamePanel.Location = new System.Drawing.Point(20, 3);
            this.actionNamePanel.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
            this.actionNamePanel.Name = "actionNamePanel";
            this.actionNamePanel.Size = new System.Drawing.Size(294, 26);
            this.actionNamePanel.TabIndex = 4;
            // 
            // actionNameLabel
            // 
            this.actionNameLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.actionNameLabel.AutoSize = true;
            this.actionNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.actionNameLabel.Location = new System.Drawing.Point(117, 6);
            this.actionNameLabel.Name = "actionNameLabel";
            this.actionNameLabel.Size = new System.Drawing.Size(75, 13);
            this.actionNameLabel.TabIndex = 0;
            this.actionNameLabel.Text = "Executing...";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(20, 35);
            this.progressBar.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(294, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 1;
            // 
            // loggerListBox
            // 
            this.loggerListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loggerListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.loggerListBox.FormattingEnabled = true;
            this.loggerListBox.Location = new System.Drawing.Point(20, 64);
            this.loggerListBox.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
            this.loggerListBox.Name = "loggerListBox";
            this.loggerListBox.Size = new System.Drawing.Size(294, 108);
            this.loggerListBox.TabIndex = 2;
            this.loggerListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.loggerListBox_DrawItem);
            this.loggerListBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.loggerListBox_MeasureItem);
            // 
            // closeButtonLayoutPanel
            // 
            this.closeButtonLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButtonLayoutPanel.Controls.Add(this.closeButton);
            this.closeButtonLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.closeButtonLayoutPanel.Location = new System.Drawing.Point(20, 178);
            this.closeButtonLayoutPanel.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
            this.closeButtonLayoutPanel.Name = "closeButtonLayoutPanel";
            this.closeButtonLayoutPanel.Size = new System.Drawing.Size(294, 30);
            this.closeButtonLayoutPanel.TabIndex = 3;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(216, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.closeButtonLayoutPanel, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.progressBar, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.actionNamePanel, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.loggerListBox, 0, 2);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 4;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(334, 211);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // PackageOperationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 211);
            this.Controls.Add(this.tableLayoutPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackageOperationDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Operation";
            this.actionNamePanel.ResumeLayout(false);
            this.actionNamePanel.PerformLayout();
            this.closeButtonLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label actionNameLabel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ListBox loggerListBox;
        private System.Windows.Forms.FlowLayoutPanel closeButtonLayoutPanel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Panel actionNamePanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    }
}