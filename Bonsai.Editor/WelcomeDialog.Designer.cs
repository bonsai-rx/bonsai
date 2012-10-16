namespace Bonsai.Editor
{
    partial class WelcomeDialog
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
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.controlPanel = new System.Windows.Forms.Panel();
            this.showWelcomeDialogCheckBox = new System.Windows.Forms.CheckBox();
            this.closeButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.controlPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Image = global::Bonsai.Editor.Properties.Resources.BonsaiWelcome;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 60);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(640, 480);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // controlPanel
            // 
            this.controlPanel.BackColor = System.Drawing.SystemColors.Control;
            this.controlPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.controlPanel.Controls.Add(this.closeButton);
            this.controlPanel.Controls.Add(this.showWelcomeDialogCheckBox);
            this.controlPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.controlPanel.Location = new System.Drawing.Point(0, 264);
            this.controlPanel.Name = "controlPanel";
            this.controlPanel.Size = new System.Drawing.Size(553, 60);
            this.controlPanel.TabIndex = 1;
            // 
            // showWelcomeDialogCheckBox
            // 
            this.showWelcomeDialogCheckBox.AutoSize = true;
            this.showWelcomeDialogCheckBox.Location = new System.Drawing.Point(12, 22);
            this.showWelcomeDialogCheckBox.Name = "showWelcomeDialogCheckBox";
            this.showWelcomeDialogCheckBox.Size = new System.Drawing.Size(145, 17);
            this.showWelcomeDialogCheckBox.TabIndex = 0;
            this.showWelcomeDialogCheckBox.Text = "Show welcome at startup";
            this.showWelcomeDialogCheckBox.UseVisualStyleBackColor = true;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(464, 18);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 3;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // WelcomeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(553, 324);
            this.Controls.Add(this.controlPanel);
            this.Controls.Add(this.pictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WelcomeDialog";
            this.ShowIcon = false;
            this.Text = "Welcome to Bonsai";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.controlPanel.ResumeLayout(false);
            this.controlPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Panel controlPanel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.CheckBox showWelcomeDialogCheckBox;
    }
}