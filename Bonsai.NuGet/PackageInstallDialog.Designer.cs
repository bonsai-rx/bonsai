namespace Bonsai.NuGet
{
    partial class PackageInstallDialog
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
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.actionNamePanel = new System.Windows.Forms.Panel();
            this.actionNameLabel = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.loggerListBox = new System.Windows.Forms.ListBox();
            this.closeButtonLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.closeButton = new System.Windows.Forms.Button();
            this.flowLayoutPanel.SuspendLayout();
            this.actionNamePanel.SuspendLayout();
            this.closeButtonLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.Controls.Add(this.actionNamePanel);
            this.flowLayoutPanel.Controls.Add(this.progressBar);
            this.flowLayoutPanel.Controls.Add(this.loggerListBox);
            this.flowLayoutPanel.Controls.Add(this.closeButtonLayoutPanel);
            this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(334, 211);
            this.flowLayoutPanel.TabIndex = 0;
            // 
            // actionNamePanel
            // 
            this.actionNamePanel.Controls.Add(this.actionNameLabel);
            this.actionNamePanel.Location = new System.Drawing.Point(20, 3);
            this.actionNamePanel.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
            this.actionNamePanel.Name = "actionNamePanel";
            this.actionNamePanel.Size = new System.Drawing.Size(297, 26);
            this.actionNamePanel.TabIndex = 4;
            // 
            // actionNameLabel
            // 
            this.actionNameLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.actionNameLabel.AutoSize = true;
            this.actionNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.actionNameLabel.Location = new System.Drawing.Point(118, 6);
            this.actionNameLabel.Name = "actionNameLabel";
            this.actionNameLabel.Size = new System.Drawing.Size(70, 13);
            this.actionNameLabel.TabIndex = 0;
            this.actionNameLabel.Text = "Installing...";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(20, 35);
            this.progressBar.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(297, 23);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar.TabIndex = 1;
            // 
            // loggerListBox
            // 
            this.loggerListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loggerListBox.FormattingEnabled = true;
            this.loggerListBox.Location = new System.Drawing.Point(20, 64);
            this.loggerListBox.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
            this.loggerListBox.Name = "loggerListBox";
            this.loggerListBox.Size = new System.Drawing.Size(297, 95);
            this.loggerListBox.TabIndex = 2;
            // 
            // closeButtonLayoutPanel
            // 
            this.closeButtonLayoutPanel.Controls.Add(this.closeButton);
            this.closeButtonLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.closeButtonLayoutPanel.Location = new System.Drawing.Point(20, 165);
            this.closeButtonLayoutPanel.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            this.closeButtonLayoutPanel.Name = "closeButtonLayoutPanel";
            this.closeButtonLayoutPanel.Size = new System.Drawing.Size(297, 36);
            this.closeButtonLayoutPanel.TabIndex = 3;
            // 
            // closeButton
            // 
            this.closeButton.Location = new System.Drawing.Point(219, 3);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // PackageInstallDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 211);
            this.Controls.Add(this.flowLayoutPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackageInstallDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "PackageInstallDialog";
            this.flowLayoutPanel.ResumeLayout(false);
            this.actionNamePanel.ResumeLayout(false);
            this.actionNamePanel.PerformLayout();
            this.closeButtonLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Label actionNameLabel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ListBox loggerListBox;
        private System.Windows.Forms.FlowLayoutPanel closeButtonLayoutPanel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Panel actionNamePanel;
    }
}