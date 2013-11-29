namespace Bonsai.NuGet
{
    partial class LicenseAcceptanceDialog
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
            this.mainFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.packageListLabel = new System.Windows.Forms.Label();
            this.packageListBox = new System.Windows.Forms.ListBox();
            this.clarificationLabel = new System.Windows.Forms.Label();
            this.actionFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.acceptButton = new System.Windows.Forms.Button();
            this.declineButton = new System.Windows.Forms.Button();
            this.mainFlowLayoutPanel.SuspendLayout();
            this.actionFlowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainFlowLayoutPanel
            // 
            this.mainFlowLayoutPanel.Controls.Add(this.packageListLabel);
            this.mainFlowLayoutPanel.Controls.Add(this.packageListBox);
            this.mainFlowLayoutPanel.Controls.Add(this.clarificationLabel);
            this.mainFlowLayoutPanel.Controls.Add(this.actionFlowLayoutPanel);
            this.mainFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.mainFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainFlowLayoutPanel.Name = "mainFlowLayoutPanel";
            this.mainFlowLayoutPanel.Size = new System.Drawing.Size(384, 411);
            this.mainFlowLayoutPanel.TabIndex = 0;
            // 
            // packageListLabel
            // 
            this.packageListLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.packageListLabel.Location = new System.Drawing.Point(10, 13);
            this.packageListLabel.Margin = new System.Windows.Forms.Padding(10, 13, 3, 0);
            this.packageListLabel.Name = "packageListLabel";
            this.packageListLabel.Size = new System.Drawing.Size(365, 13);
            this.packageListLabel.TabIndex = 0;
            this.packageListLabel.Text = "The following package(s) require a click-to-accept license:";
            // 
            // packageListBox
            // 
            this.packageListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.packageListBox.FormattingEnabled = true;
            this.packageListBox.Location = new System.Drawing.Point(10, 29);
            this.packageListBox.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.packageListBox.Name = "packageListBox";
            this.packageListBox.Size = new System.Drawing.Size(365, 277);
            this.packageListBox.TabIndex = 5;
            // 
            // clarificationLabel
            // 
            this.clarificationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clarificationLabel.AutoSize = true;
            this.clarificationLabel.Location = new System.Drawing.Point(10, 309);
            this.clarificationLabel.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
            this.clarificationLabel.Name = "clarificationLabel";
            this.clarificationLabel.Size = new System.Drawing.Size(365, 26);
            this.clarificationLabel.TabIndex = 2;
            this.clarificationLabel.Text = "By clicking \"I Accept,\" you agree to the license terms for the package(s) listed " +
    "above. If you do not agree to the license terms, click \"I Decline.\"";
            // 
            // actionFlowLayoutPanel
            // 
            this.actionFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.actionFlowLayoutPanel.Controls.Add(this.acceptButton);
            this.actionFlowLayoutPanel.Controls.Add(this.declineButton);
            this.actionFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.actionFlowLayoutPanel.Location = new System.Drawing.Point(10, 338);
            this.actionFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.actionFlowLayoutPanel.Name = "actionFlowLayoutPanel";
            this.actionFlowLayoutPanel.Size = new System.Drawing.Size(365, 33);
            this.actionFlowLayoutPanel.TabIndex = 3;
            // 
            // acceptButton
            // 
            this.acceptButton.Location = new System.Drawing.Point(287, 3);
            this.acceptButton.Name = "acceptButton";
            this.acceptButton.Size = new System.Drawing.Size(75, 23);
            this.acceptButton.TabIndex = 0;
            this.acceptButton.Text = "I Accept";
            this.acceptButton.UseVisualStyleBackColor = true;
            // 
            // declineButton
            // 
            this.declineButton.Location = new System.Drawing.Point(206, 3);
            this.declineButton.Name = "declineButton";
            this.declineButton.Size = new System.Drawing.Size(75, 23);
            this.declineButton.TabIndex = 1;
            this.declineButton.Text = "I Decline";
            this.declineButton.UseVisualStyleBackColor = true;
            // 
            // LicenseAcceptanceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 411);
            this.Controls.Add(this.mainFlowLayoutPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(350, 300);
            this.Name = "LicenseAcceptanceDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "License Acceptance";
            this.mainFlowLayoutPanel.ResumeLayout(false);
            this.mainFlowLayoutPanel.PerformLayout();
            this.actionFlowLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel mainFlowLayoutPanel;
        private System.Windows.Forms.Label packageListLabel;
        private System.Windows.Forms.Label clarificationLabel;
        private System.Windows.Forms.FlowLayoutPanel actionFlowLayoutPanel;
        private System.Windows.Forms.Button acceptButton;
        private System.Windows.Forms.Button declineButton;
        private System.Windows.Forms.ListBox packageListBox;
    }
}