namespace Bonsai.NuGet.Design
{
    partial class PackageDetails
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
            this.detailsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.versionLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.versionHeader = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.operationButton = new System.Windows.Forms.Button();
            this.createdByLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.createdByHeader = new System.Windows.Forms.Label();
            this.createdByLabel = new System.Windows.Forms.Label();
            this.lastPublishedLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lastPublishedHeader = new System.Windows.Forms.Label();
            this.lastPublishedLabel = new System.Windows.Forms.Label();
            this.downloadsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.downloadsHeader = new System.Windows.Forms.Label();
            this.downloadsLabel = new System.Windows.Forms.Label();
            this.reportAbuseLinkLabel = new System.Windows.Forms.LinkLabel();
            this.deprecationMetadataPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.deprecationMetadataLabel = new System.Windows.Forms.Label();
            this.descriptionLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.descriptionHeader = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.tagsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.tagsHeader = new System.Windows.Forms.Label();
            this.tagsLabel = new System.Windows.Forms.Label();
            this.dependenciesHeaderLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.dependenciesHeader = new System.Windows.Forms.Label();
            this.dependenciesLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.dependenciesTextBox = new System.Windows.Forms.TextBox();
            this.dependencyWarningLabel = new System.Windows.Forms.Label();
            this.packageIdLabel = new Bonsai.NuGet.Design.ImageLabel();
            this.detailsLinkLabel = new Bonsai.NuGet.Design.ImageLinkLabel();
            this.projectLinkLabel = new Bonsai.NuGet.Design.ImageLinkLabel();
            this.licenseLinkLabel = new Bonsai.NuGet.Design.ImageLinkLabel();
            this.warningImageLabel = new Bonsai.NuGet.Design.ImageLabel();
            this.detailsLayoutPanel.SuspendLayout();
            this.versionLayoutPanel.SuspendLayout();
            this.createdByLayoutPanel.SuspendLayout();
            this.lastPublishedLayoutPanel.SuspendLayout();
            this.downloadsLayoutPanel.SuspendLayout();
            this.deprecationMetadataPanel.SuspendLayout();
            this.descriptionLayoutPanel.SuspendLayout();
            this.tagsLayoutPanel.SuspendLayout();
            this.dependenciesHeaderLayoutPanel.SuspendLayout();
            this.dependenciesLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // detailsLayoutPanel
            // 
            this.detailsLayoutPanel.AutoScroll = true;
            this.detailsLayoutPanel.Controls.Add(this.packageIdLabel);
            this.detailsLayoutPanel.Controls.Add(this.versionLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.createdByLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.lastPublishedLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.downloadsLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.detailsLinkLabel);
            this.detailsLayoutPanel.Controls.Add(this.projectLinkLabel);
            this.detailsLayoutPanel.Controls.Add(this.licenseLinkLabel);
            this.detailsLayoutPanel.Controls.Add(this.reportAbuseLinkLabel);
            this.detailsLayoutPanel.Controls.Add(this.deprecationMetadataPanel);
            this.detailsLayoutPanel.Controls.Add(this.descriptionLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.tagsLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.dependenciesHeaderLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.dependenciesLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.dependencyWarningLabel);
            this.detailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.detailsLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.detailsLayoutPanel.Name = "detailsLayoutPanel";
            this.detailsLayoutPanel.Size = new System.Drawing.Size(240, 450);
            this.detailsLayoutPanel.TabIndex = 0;
            this.detailsLayoutPanel.WrapContents = false;
            // 
            // versionLayoutPanel
            // 
            this.versionLayoutPanel.AutoSize = true;
            this.versionLayoutPanel.Controls.Add(this.versionHeader);
            this.versionLayoutPanel.Controls.Add(this.versionLabel);
            this.versionLayoutPanel.Controls.Add(this.operationButton);
            this.versionLayoutPanel.Location = new System.Drawing.Point(3, 27);
            this.versionLayoutPanel.Name = "versionLayoutPanel";
            this.versionLayoutPanel.Size = new System.Drawing.Size(206, 23);
            this.versionLayoutPanel.TabIndex = 2;
            // 
            // versionHeader
            // 
            this.versionHeader.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.versionHeader.AutoSize = true;
            this.versionHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionHeader.Location = new System.Drawing.Point(3, 5);
            this.versionHeader.Name = "versionHeader";
            this.versionHeader.Size = new System.Drawing.Size(53, 13);
            this.versionHeader.TabIndex = 0;
            this.versionHeader.Text = "Version:";
            // 
            // versionLabel
            // 
            this.versionLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(62, 5);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(35, 13);
            this.versionLabel.TabIndex = 1;
            this.versionLabel.Text = "label4";
            // 
            // operationButton
            // 
            this.operationButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.operationButton.Location = new System.Drawing.Point(103, 0);
            this.operationButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.operationButton.Name = "operationButton";
            this.operationButton.Size = new System.Drawing.Size(100, 23);
            this.operationButton.TabIndex = 0;
            this.operationButton.Text = "Operation";
            this.operationButton.UseVisualStyleBackColor = true;
            this.operationButton.Click += new System.EventHandler(this.operationButton_Click);
            // 
            // createdByLayoutPanel
            // 
            this.createdByLayoutPanel.AutoSize = true;
            this.createdByLayoutPanel.Controls.Add(this.createdByHeader);
            this.createdByLayoutPanel.Controls.Add(this.createdByLabel);
            this.createdByLayoutPanel.Location = new System.Drawing.Point(3, 56);
            this.createdByLayoutPanel.Name = "createdByLayoutPanel";
            this.createdByLayoutPanel.Size = new System.Drawing.Size(119, 13);
            this.createdByLayoutPanel.TabIndex = 0;
            // 
            // createdByHeader
            // 
            this.createdByHeader.AutoSize = true;
            this.createdByHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.createdByHeader.Location = new System.Drawing.Point(3, 0);
            this.createdByHeader.Name = "createdByHeader";
            this.createdByHeader.Size = new System.Drawing.Size(72, 13);
            this.createdByHeader.TabIndex = 0;
            this.createdByHeader.Text = "Created by:";
            // 
            // createdByLabel
            // 
            this.createdByLabel.AutoSize = true;
            this.createdByLabel.Location = new System.Drawing.Point(81, 0);
            this.createdByLabel.Name = "createdByLabel";
            this.createdByLabel.Size = new System.Drawing.Size(35, 13);
            this.createdByLabel.TabIndex = 1;
            this.createdByLabel.Text = "label2";
            // 
            // lastPublishedLayoutPanel
            // 
            this.lastPublishedLayoutPanel.AutoSize = true;
            this.lastPublishedLayoutPanel.Controls.Add(this.lastPublishedHeader);
            this.lastPublishedLayoutPanel.Controls.Add(this.lastPublishedLabel);
            this.lastPublishedLayoutPanel.Location = new System.Drawing.Point(3, 75);
            this.lastPublishedLayoutPanel.Name = "lastPublishedLayoutPanel";
            this.lastPublishedLayoutPanel.Size = new System.Drawing.Size(141, 13);
            this.lastPublishedLayoutPanel.TabIndex = 3;
            // 
            // lastPublishedHeader
            // 
            this.lastPublishedHeader.AutoSize = true;
            this.lastPublishedHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lastPublishedHeader.Location = new System.Drawing.Point(3, 0);
            this.lastPublishedHeader.Name = "lastPublishedHeader";
            this.lastPublishedHeader.Size = new System.Drawing.Size(94, 13);
            this.lastPublishedHeader.TabIndex = 0;
            this.lastPublishedHeader.Text = "Last Published:";
            // 
            // lastPublishedLabel
            // 
            this.lastPublishedLabel.AutoSize = true;
            this.lastPublishedLabel.Location = new System.Drawing.Point(103, 0);
            this.lastPublishedLabel.Name = "lastPublishedLabel";
            this.lastPublishedLabel.Size = new System.Drawing.Size(35, 13);
            this.lastPublishedLabel.TabIndex = 1;
            this.lastPublishedLabel.Text = "label5";
            // 
            // downloadsLayoutPanel
            // 
            this.downloadsLayoutPanel.AutoSize = true;
            this.downloadsLayoutPanel.Controls.Add(this.downloadsHeader);
            this.downloadsLayoutPanel.Controls.Add(this.downloadsLabel);
            this.downloadsLayoutPanel.Location = new System.Drawing.Point(3, 94);
            this.downloadsLayoutPanel.Name = "downloadsLayoutPanel";
            this.downloadsLayoutPanel.Size = new System.Drawing.Size(120, 13);
            this.downloadsLayoutPanel.TabIndex = 11;
            // 
            // downloadsHeader
            // 
            this.downloadsHeader.AutoSize = true;
            this.downloadsHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.downloadsHeader.Location = new System.Drawing.Point(3, 0);
            this.downloadsHeader.Name = "downloadsHeader";
            this.downloadsHeader.Size = new System.Drawing.Size(73, 13);
            this.downloadsHeader.TabIndex = 0;
            this.downloadsHeader.Text = "Downloads:";
            // 
            // downloadsLabel
            // 
            this.downloadsLabel.AutoSize = true;
            this.downloadsLabel.Location = new System.Drawing.Point(82, 0);
            this.downloadsLabel.Name = "downloadsLabel";
            this.downloadsLabel.Size = new System.Drawing.Size(35, 13);
            this.downloadsLabel.TabIndex = 1;
            this.downloadsLabel.Text = "label8";
            // 
            // reportAbuseLinkLabel
            // 
            this.reportAbuseLinkLabel.AutoSize = true;
            this.reportAbuseLinkLabel.ForeColor = System.Drawing.Color.Blue;
            this.reportAbuseLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.reportAbuseLinkLabel.Location = new System.Drawing.Point(6, 164);
            this.reportAbuseLinkLabel.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.reportAbuseLinkLabel.Name = "reportAbuseLinkLabel";
            this.reportAbuseLinkLabel.Size = new System.Drawing.Size(72, 13);
            this.reportAbuseLinkLabel.TabIndex = 6;
            this.reportAbuseLinkLabel.TabStop = true;
            this.reportAbuseLinkLabel.Text = "Report Abuse";
            this.reportAbuseLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.reportAbuseLinkLabel.MouseEnter += new System.EventHandler(this.linkLabel_MouseEnter);
            this.reportAbuseLinkLabel.MouseLeave += new System.EventHandler(this.linkLabel_MouseLeave);
            // 
            // deprecationMetadataPanel
            // 
            this.deprecationMetadataPanel.AutoSize = true;
            this.deprecationMetadataPanel.Controls.Add(this.warningImageLabel);
            this.deprecationMetadataPanel.Controls.Add(this.deprecationMetadataLabel);
            this.deprecationMetadataPanel.Location = new System.Drawing.Point(3, 183);
            this.deprecationMetadataPanel.Name = "deprecationMetadataPanel";
            this.deprecationMetadataPanel.Size = new System.Drawing.Size(141, 17);
            this.deprecationMetadataPanel.TabIndex = 15;
            // 
            // deprecationMetadataLabel
            // 
            this.deprecationMetadataLabel.AutoSize = true;
            this.deprecationMetadataLabel.Location = new System.Drawing.Point(103, 0);
            this.deprecationMetadataLabel.Name = "deprecationMetadataLabel";
            this.deprecationMetadataLabel.Size = new System.Drawing.Size(35, 13);
            this.deprecationMetadataLabel.TabIndex = 8;
            this.deprecationMetadataLabel.Text = "label9";
            // 
            // descriptionLayoutPanel
            // 
            this.descriptionLayoutPanel.AutoSize = true;
            this.descriptionLayoutPanel.Controls.Add(this.descriptionHeader);
            this.descriptionLayoutPanel.Controls.Add(this.descriptionLabel);
            this.descriptionLayoutPanel.Location = new System.Drawing.Point(3, 206);
            this.descriptionLayoutPanel.Name = "descriptionLayoutPanel";
            this.descriptionLayoutPanel.Size = new System.Drawing.Size(122, 13);
            this.descriptionLayoutPanel.TabIndex = 10;
            // 
            // descriptionHeader
            // 
            this.descriptionHeader.AutoSize = true;
            this.descriptionHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionHeader.Location = new System.Drawing.Point(3, 0);
            this.descriptionHeader.Name = "descriptionHeader";
            this.descriptionHeader.Size = new System.Drawing.Size(75, 13);
            this.descriptionHeader.TabIndex = 7;
            this.descriptionHeader.Text = "Description:";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(84, 0);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(35, 13);
            this.descriptionLabel.TabIndex = 8;
            this.descriptionLabel.Text = "label6";
            // 
            // tagsLayoutPanel
            // 
            this.tagsLayoutPanel.AutoSize = true;
            this.tagsLayoutPanel.Controls.Add(this.tagsHeader);
            this.tagsLayoutPanel.Controls.Add(this.tagsLabel);
            this.tagsLayoutPanel.Location = new System.Drawing.Point(3, 225);
            this.tagsLayoutPanel.Name = "tagsLayoutPanel";
            this.tagsLayoutPanel.Size = new System.Drawing.Size(86, 13);
            this.tagsLayoutPanel.TabIndex = 9;
            // 
            // tagsHeader
            // 
            this.tagsHeader.AutoSize = true;
            this.tagsHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tagsHeader.Location = new System.Drawing.Point(3, 0);
            this.tagsHeader.Name = "tagsHeader";
            this.tagsHeader.Size = new System.Drawing.Size(39, 13);
            this.tagsHeader.TabIndex = 0;
            this.tagsHeader.Text = "Tags:";
            // 
            // tagsLabel
            // 
            this.tagsLabel.AutoSize = true;
            this.tagsLabel.Location = new System.Drawing.Point(48, 0);
            this.tagsLabel.Name = "tagsLabel";
            this.tagsLabel.Size = new System.Drawing.Size(35, 13);
            this.tagsLabel.TabIndex = 1;
            this.tagsLabel.Text = "label7";
            // 
            // dependenciesHeaderLayoutPanel
            // 
            this.dependenciesHeaderLayoutPanel.AutoSize = true;
            this.dependenciesHeaderLayoutPanel.Controls.Add(this.dependenciesHeader);
            this.dependenciesHeaderLayoutPanel.Location = new System.Drawing.Point(3, 244);
            this.dependenciesHeaderLayoutPanel.Name = "dependenciesHeaderLayoutPanel";
            this.dependenciesHeaderLayoutPanel.Size = new System.Drawing.Size(98, 13);
            this.dependenciesHeaderLayoutPanel.TabIndex = 12;
            // 
            // dependenciesHeader
            // 
            this.dependenciesHeader.AutoSize = true;
            this.dependenciesHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dependenciesHeader.Location = new System.Drawing.Point(3, 0);
            this.dependenciesHeader.Name = "dependenciesHeader";
            this.dependenciesHeader.Size = new System.Drawing.Size(92, 13);
            this.dependenciesHeader.TabIndex = 0;
            this.dependenciesHeader.Text = "Dependencies:";
            // 
            // dependenciesLayoutPanel
            // 
            this.dependenciesLayoutPanel.AutoSize = true;
            this.dependenciesLayoutPanel.Controls.Add(this.dependenciesTextBox);
            this.dependenciesLayoutPanel.Location = new System.Drawing.Point(3, 263);
            this.dependenciesLayoutPanel.Name = "dependenciesLayoutPanel";
            this.dependenciesLayoutPanel.Size = new System.Drawing.Size(188, 26);
            this.dependenciesLayoutPanel.TabIndex = 13;
            // 
            // dependenciesTextBox
            // 
            this.dependenciesTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dependenciesTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.dependenciesTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dependenciesTextBox.Location = new System.Drawing.Point(6, 3);
            this.dependenciesTextBox.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.dependenciesTextBox.MinimumSize = new System.Drawing.Size(179, 20);
            this.dependenciesTextBox.Multiline = true;
            this.dependenciesTextBox.Name = "dependenciesTextBox";
            this.dependenciesTextBox.Size = new System.Drawing.Size(179, 20);
            this.dependenciesTextBox.TabIndex = 0;
            this.dependenciesTextBox.TextChanged += new System.EventHandler(this.dependenciesTextBox_TextChanged);
            // 
            // dependencyWarningLabel
            // 
            this.dependencyWarningLabel.AutoSize = true;
            this.dependencyWarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dependencyWarningLabel.Location = new System.Drawing.Point(3, 292);
            this.dependencyWarningLabel.Name = "dependencyWarningLabel";
            this.dependencyWarningLabel.Size = new System.Drawing.Size(0, 13);
            this.dependencyWarningLabel.TabIndex = 14;
            // 
            // packageIdLabel
            // 
            this.packageIdLabel.AutoSize = true;
            this.packageIdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.packageIdLabel.Image = null;
            this.packageIdLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.packageIdLabel.ImageIndex = 0;
            this.packageIdLabel.Location = new System.Drawing.Point(3, 3);
            this.packageIdLabel.Margin = new System.Windows.Forms.Padding(3);
            this.packageIdLabel.Name = "packageIdLabel";
            this.packageIdLabel.Size = new System.Drawing.Size(66, 18);
            this.packageIdLabel.TabIndex = 12;
            this.packageIdLabel.Text = "Package";
            this.packageIdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // detailsLinkLabel
            // 
            this.detailsLinkLabel.AutoSize = true;
            this.detailsLinkLabel.ForeColor = System.Drawing.Color.Blue;
            this.detailsLinkLabel.Image = global::Bonsai.NuGet.Design.Properties.Resources.PackageImage;
            this.detailsLinkLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailsLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.detailsLinkLabel.Location = new System.Drawing.Point(6, 110);
            this.detailsLinkLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.detailsLinkLabel.Name = "detailsLinkLabel";
            this.detailsLinkLabel.Size = new System.Drawing.Size(102, 17);
            this.detailsLinkLabel.TabIndex = 18;
            this.detailsLinkLabel.TabStop = true;
            this.detailsLinkLabel.Text = "Package Details";
            this.detailsLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // projectLinkLabel
            // 
            this.projectLinkLabel.AutoSize = true;
            this.projectLinkLabel.ForeColor = System.Drawing.Color.Blue;
            this.projectLinkLabel.Image = global::Bonsai.NuGet.Design.Properties.Resources.WebImage;
            this.projectLinkLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.projectLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.projectLinkLabel.Location = new System.Drawing.Point(6, 127);
            this.projectLinkLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.projectLinkLabel.Name = "projectLinkLabel";
            this.projectLinkLabel.Size = new System.Drawing.Size(99, 17);
            this.projectLinkLabel.TabIndex = 16;
            this.projectLinkLabel.TabStop = true;
            this.projectLinkLabel.Text = "Project Website";
            this.projectLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.projectLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.projectLinkLabel.MouseEnter += new System.EventHandler(this.linkLabel_MouseEnter);
            this.projectLinkLabel.MouseLeave += new System.EventHandler(this.linkLabel_MouseLeave);
            // 
            // licenseLinkLabel
            // 
            this.licenseLinkLabel.AutoSize = true;
            this.licenseLinkLabel.ForeColor = System.Drawing.Color.Blue;
            this.licenseLinkLabel.Image = global::Bonsai.NuGet.Design.Properties.Resources.LicenseImage;
            this.licenseLinkLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.licenseLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.licenseLinkLabel.Location = new System.Drawing.Point(6, 144);
            this.licenseLinkLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.licenseLinkLabel.Name = "licenseLinkLabel";
            this.licenseLinkLabel.Size = new System.Drawing.Size(82, 17);
            this.licenseLinkLabel.TabIndex = 17;
            this.licenseLinkLabel.TabStop = true;
            this.licenseLinkLabel.Text = "License Info";
            this.licenseLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.licenseLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.licenseLinkLabel.MouseEnter += new System.EventHandler(this.linkLabel_MouseEnter);
            this.licenseLinkLabel.MouseLeave += new System.EventHandler(this.linkLabel_MouseLeave);
            // 
            // warningImageLabel
            // 
            this.warningImageLabel.AutoSize = true;
            this.warningImageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningImageLabel.Image = global::Bonsai.NuGet.Design.Properties.Resources.WarningImage;
            this.warningImageLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.warningImageLabel.ImageIndex = 0;
            this.warningImageLabel.Location = new System.Drawing.Point(3, 0);
            this.warningImageLabel.Name = "warningImageLabel";
            this.warningImageLabel.Size = new System.Drawing.Size(94, 17);
            this.warningImageLabel.TabIndex = 11;
            this.warningImageLabel.Text = "Deprecated:";
            this.warningImageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // PackageDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.detailsLayoutPanel);
            this.Name = "PackageDetails";
            this.Size = new System.Drawing.Size(240, 450);
            this.detailsLayoutPanel.ResumeLayout(false);
            this.detailsLayoutPanel.PerformLayout();
            this.versionLayoutPanel.ResumeLayout(false);
            this.versionLayoutPanel.PerformLayout();
            this.createdByLayoutPanel.ResumeLayout(false);
            this.createdByLayoutPanel.PerformLayout();
            this.lastPublishedLayoutPanel.ResumeLayout(false);
            this.lastPublishedLayoutPanel.PerformLayout();
            this.downloadsLayoutPanel.ResumeLayout(false);
            this.downloadsLayoutPanel.PerformLayout();
            this.deprecationMetadataPanel.ResumeLayout(false);
            this.deprecationMetadataPanel.PerformLayout();
            this.descriptionLayoutPanel.ResumeLayout(false);
            this.descriptionLayoutPanel.PerformLayout();
            this.tagsLayoutPanel.ResumeLayout(false);
            this.tagsLayoutPanel.PerformLayout();
            this.dependenciesHeaderLayoutPanel.ResumeLayout(false);
            this.dependenciesHeaderLayoutPanel.PerformLayout();
            this.dependenciesLayoutPanel.ResumeLayout(false);
            this.dependenciesLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel detailsLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel createdByLayoutPanel;
        private System.Windows.Forms.Label createdByHeader;
        private System.Windows.Forms.Label createdByLabel;
        private System.Windows.Forms.FlowLayoutPanel lastPublishedLayoutPanel;
        private System.Windows.Forms.Label lastPublishedHeader;
        private System.Windows.Forms.Label lastPublishedLabel;
        private System.Windows.Forms.LinkLabel reportAbuseLinkLabel;
        private System.Windows.Forms.Label descriptionHeader;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.FlowLayoutPanel tagsLayoutPanel;
        private System.Windows.Forms.Label tagsHeader;
        private System.Windows.Forms.Label tagsLabel;
        private System.Windows.Forms.FlowLayoutPanel descriptionLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel downloadsLayoutPanel;
        private System.Windows.Forms.Label downloadsHeader;
        private System.Windows.Forms.Label downloadsLabel;
        private System.Windows.Forms.FlowLayoutPanel dependenciesHeaderLayoutPanel;
        private System.Windows.Forms.Label dependenciesHeader;
        private System.Windows.Forms.FlowLayoutPanel dependenciesLayoutPanel;
        private System.Windows.Forms.TextBox dependenciesTextBox;
        private System.Windows.Forms.Label dependencyWarningLabel;
        private System.Windows.Forms.FlowLayoutPanel deprecationMetadataPanel;
        private System.Windows.Forms.Label deprecationMetadataLabel;
        private ImageLabel warningImageLabel;
        private ImageLinkLabel projectLinkLabel;
        private ImageLinkLabel licenseLinkLabel;
        private ImageLabel packageIdLabel;
        private ImageLinkLabel detailsLinkLabel;
        private System.Windows.Forms.FlowLayoutPanel versionLayoutPanel;
        private System.Windows.Forms.Label versionHeader;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Button operationButton;
    }
}
