namespace Bonsai.NuGet
{
    partial class PackageBuilderDialog
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
            this.metadataProperties = new Bonsai.Design.PropertyGrid();
            this.contentView = new System.Windows.Forms.TreeView();
            this.metadataLabel = new System.Windows.Forms.Label();
            this.contentLabel = new System.Windows.Forms.Label();
            this.exportButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // metadataProperties
            // 
            this.metadataProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metadataProperties.DisabledItemForeColor = System.Drawing.SystemColors.ControlText;
            this.metadataProperties.Location = new System.Drawing.Point(12, 25);
            this.metadataProperties.Name = "metadataProperties";
            this.metadataProperties.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.metadataProperties.Size = new System.Drawing.Size(283, 375);
            this.metadataProperties.TabIndex = 2;
            // 
            // contentView
            // 
            this.contentView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right))));
            this.contentView.Location = new System.Drawing.Point(301, 51);
            this.contentView.Name = "contentView";
            this.contentView.Size = new System.Drawing.Size(311, 349);
            this.contentView.TabIndex = 3;
            // 
            // metadataLabel
            // 
            this.metadataLabel.AutoSize = true;
            this.metadataLabel.Location = new System.Drawing.Point(12, 9);
            this.metadataLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.metadataLabel.Name = "metadataLabel";
            this.metadataLabel.Size = new System.Drawing.Size(100, 13);
            this.metadataLabel.TabIndex = 2;
            this.metadataLabel.Text = "Package metadata:";
            // 
            // contentLabel
            // 
            this.contentLabel.AutoSize = true;
            this.contentLabel.Location = new System.Drawing.Point(301, 9);
            this.contentLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.contentLabel.Name = "contentLabel";
            this.contentLabel.Size = new System.Drawing.Size(97, 13);
            this.contentLabel.TabIndex = 3;
            this.contentLabel.Text = "Package contents:";
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exportButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.exportButton.Location = new System.Drawing.Point(456, 406);
            this.exportButton.Margin = new System.Windows.Forms.Padding(6);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 0;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(537, 406);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(6);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "NuGet package file (*.nupkg)|*.nupkg";
            // 
            // PackageBuilderDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.contentLabel);
            this.Controls.Add(this.metadataLabel);
            this.Controls.Add(this.contentView);
            this.Controls.Add(this.metadataProperties);
            this.MinimumSize = new System.Drawing.Size(500, 400);
            this.Name = "PackageBuilderDialog";
            this.ShowIcon = false;
            this.Text = "Bonsai - Export Package";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Bonsai.Design.PropertyGrid metadataProperties;
        private System.Windows.Forms.TreeView contentView;
        private System.Windows.Forms.Label metadataLabel;
        private System.Windows.Forms.Label contentLabel;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}