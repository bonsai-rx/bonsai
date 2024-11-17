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
            this.components = new System.ComponentModel.Container();
            this.detailsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.packageIdPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.packageIdLabel = new Bonsai.NuGet.Design.ImageLabel();
            this.prefixReservedIcon = new System.Windows.Forms.PictureBox();
            this.installedVersionLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.installedHeader = new System.Windows.Forms.Label();
            this.installedVersionTextBox = new System.Windows.Forms.TextBox();
            this.uninstallButton = new System.Windows.Forms.Button();
            this.versionLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.versionHeader = new System.Windows.Forms.Label();
            this.versionComboBox = new System.Windows.Forms.ComboBox();
            this.operationButton = new System.Windows.Forms.Button();
            this.deprecationMetadataPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.warningImageLabel = new Bonsai.NuGet.Design.ImageLabel();
            this.deprecationMetadataLabel = new System.Windows.Forms.Label();
            this.alternatePackagePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.alternatePackageHeader = new System.Windows.Forms.Label();
            this.alternatePackageLinkLabel = new System.Windows.Forms.LinkLabel();
            this.descriptionLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.descriptionHeader = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.createdByLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.createdByHeader = new System.Windows.Forms.Label();
            this.createdByLabel = new System.Windows.Forms.Label();
            this.lastPublishedLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.lastPublishedHeader = new System.Windows.Forms.Label();
            this.lastPublishedLabel = new System.Windows.Forms.Label();
            this.downloadsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.downloadsHeader = new System.Windows.Forms.Label();
            this.downloadsLabel = new System.Windows.Forms.Label();
            this.detailsLinkLabel = new Bonsai.NuGet.Design.ImageLinkLabel();
            this.projectLinkLabel = new Bonsai.NuGet.Design.ImageLinkLabel();
            this.licenseLinkLabel = new Bonsai.NuGet.Design.ImageLinkLabel();
            this.reportAbuseLinkLabel = new System.Windows.Forms.LinkLabel();
            this.tagsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.tagsHeader = new System.Windows.Forms.Label();
            this.tagsLabel = new System.Windows.Forms.Label();
            this.dependenciesHeaderLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.dependenciesHeader = new System.Windows.Forms.Label();
            this.dependenciesLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.dependenciesTextBox = new System.Windows.Forms.TextBox();
            this.dependencyWarningLabel = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.detailsLayoutPanel.SuspendLayout();
            this.packageIdPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.prefixReservedIcon)).BeginInit();
            this.installedVersionLayoutPanel.SuspendLayout();
            this.versionLayoutPanel.SuspendLayout();
            this.deprecationMetadataPanel.SuspendLayout();
            this.alternatePackagePanel.SuspendLayout();
            this.descriptionLayoutPanel.SuspendLayout();
            this.createdByLayoutPanel.SuspendLayout();
            this.lastPublishedLayoutPanel.SuspendLayout();
            this.downloadsLayoutPanel.SuspendLayout();
            this.tagsLayoutPanel.SuspendLayout();
            this.dependenciesHeaderLayoutPanel.SuspendLayout();
            this.dependenciesLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // detailsLayoutPanel
            // 
            this.detailsLayoutPanel.AutoScroll = true;
            this.detailsLayoutPanel.Controls.Add(this.packageIdPanel);
            this.detailsLayoutPanel.Controls.Add(this.installedVersionLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.versionLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.deprecationMetadataPanel);
            this.detailsLayoutPanel.Controls.Add(this.alternatePackagePanel);
            this.detailsLayoutPanel.Controls.Add(this.descriptionLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.createdByLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.lastPublishedLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.downloadsLayoutPanel);
            this.detailsLayoutPanel.Controls.Add(this.detailsLinkLabel);
            this.detailsLayoutPanel.Controls.Add(this.projectLinkLabel);
            this.detailsLayoutPanel.Controls.Add(this.licenseLinkLabel);
            this.detailsLayoutPanel.Controls.Add(this.reportAbuseLinkLabel);
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
            // packageIdPanel
            // 
            this.packageIdPanel.AutoSize = true;
            this.packageIdPanel.Controls.Add(this.packageIdLabel);
            this.packageIdPanel.Controls.Add(this.prefixReservedIcon);
            this.packageIdPanel.Location = new System.Drawing.Point(0, 0);
            this.packageIdPanel.Margin = new System.Windows.Forms.Padding(0);
            this.packageIdPanel.Name = "packageIdPanel";
            this.packageIdPanel.Size = new System.Drawing.Size(88, 24);
            this.packageIdPanel.TabIndex = 0;
            // 
            // packageIdLabel
            // 
            this.packageIdLabel.AutoSize = true;
            this.packageIdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.packageIdLabel.Image = null;
            this.packageIdLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.packageIdLabel.ImageIndex = 0;
            this.packageIdLabel.Location = new System.Drawing.Point(3, 3);
            this.packageIdLabel.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.packageIdLabel.Name = "packageIdLabel";
            this.packageIdLabel.Size = new System.Drawing.Size(66, 18);
            this.packageIdLabel.TabIndex = 0;
            this.packageIdLabel.Text = "Package";
            this.packageIdLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // prefixReservedIcon
            // 
            this.prefixReservedIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.prefixReservedIcon.Image = global::Bonsai.NuGet.Design.Properties.Resources.PrefixReservedImage;
            this.prefixReservedIcon.Location = new System.Drawing.Point(69, 3);
            this.prefixReservedIcon.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.prefixReservedIcon.Name = "prefixReservedIcon";
            this.prefixReservedIcon.Size = new System.Drawing.Size(16, 18);
            this.prefixReservedIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.prefixReservedIcon.TabIndex = 13;
            this.prefixReservedIcon.TabStop = false;
            // 
            // installedVersionLayoutPanel
            // 
            this.installedVersionLayoutPanel.AutoSize = true;
            this.installedVersionLayoutPanel.Controls.Add(this.installedHeader);
            this.installedVersionLayoutPanel.Controls.Add(this.installedVersionTextBox);
            this.installedVersionLayoutPanel.Controls.Add(this.uninstallButton);
            this.installedVersionLayoutPanel.Location = new System.Drawing.Point(3, 27);
            this.installedVersionLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.installedVersionLayoutPanel.Name = "installedVersionLayoutPanel";
            this.installedVersionLayoutPanel.Size = new System.Drawing.Size(171, 49);
            this.installedVersionLayoutPanel.TabIndex = 1;
            // 
            // installedHeader
            // 
            this.installedHeader.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.installedHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.installedHeader.Location = new System.Drawing.Point(3, 6);
            this.installedHeader.Name = "installedHeader";
            this.installedHeader.Size = new System.Drawing.Size(61, 13);
            this.installedHeader.TabIndex = 0;
            this.installedHeader.Text = "Installed:";
            // 
            // installedVersionTextBox
            // 
            this.installedVersionTextBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.installedVersionTextBox.Location = new System.Drawing.Point(69, 3);
            this.installedVersionTextBox.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.installedVersionTextBox.Name = "installedVersionTextBox";
            this.installedVersionTextBox.ReadOnly = true;
            this.installedVersionTextBox.Size = new System.Drawing.Size(100, 20);
            this.installedVersionTextBox.TabIndex = 1;
            // 
            // uninstallButton
            // 
            this.uninstallButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.uninstallButton.Location = new System.Drawing.Point(3, 26);
            this.uninstallButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.uninstallButton.Name = "uninstallButton";
            this.uninstallButton.Size = new System.Drawing.Size(100, 23);
            this.uninstallButton.TabIndex = 2;
            this.uninstallButton.Text = "Uninstall";
            this.uninstallButton.UseVisualStyleBackColor = true;
            this.uninstallButton.Click += new System.EventHandler(this.uninstallButton_Click);
            // 
            // versionLayoutPanel
            // 
            this.versionLayoutPanel.AutoSize = true;
            this.versionLayoutPanel.Controls.Add(this.versionHeader);
            this.versionLayoutPanel.Controls.Add(this.versionComboBox);
            this.versionLayoutPanel.Controls.Add(this.operationButton);
            this.versionLayoutPanel.Location = new System.Drawing.Point(3, 76);
            this.versionLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.versionLayoutPanel.Name = "versionLayoutPanel";
            this.versionLayoutPanel.Size = new System.Drawing.Size(173, 50);
            this.versionLayoutPanel.TabIndex = 2;
            // 
            // versionHeader
            // 
            this.versionHeader.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.versionHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionHeader.Location = new System.Drawing.Point(3, 7);
            this.versionHeader.Name = "versionHeader";
            this.versionHeader.Size = new System.Drawing.Size(61, 13);
            this.versionHeader.TabIndex = 0;
            this.versionHeader.Text = "Version:";
            // 
            // versionComboBox
            // 
            this.versionComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.versionComboBox.FormattingEnabled = true;
            this.versionComboBox.Location = new System.Drawing.Point(70, 3);
            this.versionComboBox.Name = "versionComboBox";
            this.versionComboBox.Size = new System.Drawing.Size(100, 21);
            this.versionComboBox.TabIndex = 1;
            this.versionComboBox.SelectedIndexChanged += new System.EventHandler(this.versionComboBox_SelectedIndexChanged);
            this.versionComboBox.TextChanged += new System.EventHandler(this.versionComboBox_TextChanged);
            // 
            // operationButton
            // 
            this.operationButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.operationButton.Location = new System.Drawing.Point(3, 27);
            this.operationButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.operationButton.Name = "operationButton";
            this.operationButton.Size = new System.Drawing.Size(100, 23);
            this.operationButton.TabIndex = 2;
            this.operationButton.Text = "Operation";
            this.operationButton.UseVisualStyleBackColor = true;
            this.operationButton.Click += new System.EventHandler(this.operationButton_Click);
            // 
            // deprecationMetadataPanel
            // 
            this.deprecationMetadataPanel.AutoSize = true;
            this.deprecationMetadataPanel.Controls.Add(this.warningImageLabel);
            this.deprecationMetadataPanel.Controls.Add(this.deprecationMetadataLabel);
            this.deprecationMetadataPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.deprecationMetadataPanel.Location = new System.Drawing.Point(3, 132);
            this.deprecationMetadataPanel.Name = "deprecationMetadataPanel";
            this.deprecationMetadataPanel.Size = new System.Drawing.Size(96, 36);
            this.deprecationMetadataPanel.TabIndex = 3;
            // 
            // warningImageLabel
            // 
            this.warningImageLabel.AutoSize = true;
            this.warningImageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.warningImageLabel.Image = global::Bonsai.NuGet.Design.Properties.Resources.WarningImage;
            this.warningImageLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.warningImageLabel.ImageIndex = 0;
            this.warningImageLabel.Location = new System.Drawing.Point(3, 3);
            this.warningImageLabel.Margin = new System.Windows.Forms.Padding(3);
            this.warningImageLabel.Name = "warningImageLabel";
            this.warningImageLabel.Size = new System.Drawing.Size(90, 17);
            this.warningImageLabel.TabIndex = 0;
            this.warningImageLabel.Text = "Deprecated";
            this.warningImageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // deprecationMetadataLabel
            // 
            this.deprecationMetadataLabel.AutoSize = true;
            this.deprecationMetadataLabel.Location = new System.Drawing.Point(3, 23);
            this.deprecationMetadataLabel.Name = "deprecationMetadataLabel";
            this.deprecationMetadataLabel.Size = new System.Drawing.Size(84, 13);
            this.deprecationMetadataLabel.TabIndex = 1;
            this.deprecationMetadataLabel.Text = "deprecationText";
            // 
            // alternatePackagePanel
            // 
            this.alternatePackagePanel.AutoSize = true;
            this.alternatePackagePanel.Controls.Add(this.alternatePackageHeader);
            this.alternatePackagePanel.Controls.Add(this.alternatePackageLinkLabel);
            this.alternatePackagePanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.alternatePackagePanel.Location = new System.Drawing.Point(3, 174);
            this.alternatePackagePanel.Name = "alternatePackagePanel";
            this.alternatePackagePanel.Size = new System.Drawing.Size(117, 38);
            this.alternatePackagePanel.TabIndex = 4;
            // 
            // alternatePackageHeader
            // 
            this.alternatePackageHeader.AutoSize = true;
            this.alternatePackageHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.alternatePackageHeader.Location = new System.Drawing.Point(3, 3);
            this.alternatePackageHeader.Margin = new System.Windows.Forms.Padding(3);
            this.alternatePackageHeader.Name = "alternatePackageHeader";
            this.alternatePackageHeader.Size = new System.Drawing.Size(111, 13);
            this.alternatePackageHeader.TabIndex = 0;
            this.alternatePackageHeader.Text = "Alternate package";
            // 
            // alternatePackageLinkLabel
            // 
            this.alternatePackageLinkLabel.AutoSize = true;
            this.alternatePackageLinkLabel.ForeColor = System.Drawing.Color.Blue;
            this.alternatePackageLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.alternatePackageLinkLabel.Location = new System.Drawing.Point(3, 22);
            this.alternatePackageLinkLabel.Margin = new System.Windows.Forms.Padding(3);
            this.alternatePackageLinkLabel.Name = "alternatePackageLinkLabel";
            this.alternatePackageLinkLabel.Size = new System.Drawing.Size(100, 13);
            this.alternatePackageLinkLabel.TabIndex = 1;
            this.alternatePackageLinkLabel.TabStop = true;
            this.alternatePackageLinkLabel.Text = "alternatePackageId";
            this.alternatePackageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.alternatePackageLinkLabel_LinkClicked);
            this.alternatePackageLinkLabel.MouseEnter += new System.EventHandler(this.linkLabel_MouseEnter);
            this.alternatePackageLinkLabel.MouseLeave += new System.EventHandler(this.linkLabel_MouseLeave);
            // 
            // descriptionLayoutPanel
            // 
            this.descriptionLayoutPanel.AutoSize = true;
            this.descriptionLayoutPanel.Controls.Add(this.descriptionHeader);
            this.descriptionLayoutPanel.Controls.Add(this.descriptionLabel);
            this.descriptionLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.descriptionLayoutPanel.Location = new System.Drawing.Point(3, 218);
            this.descriptionLayoutPanel.Name = "descriptionLayoutPanel";
            this.descriptionLayoutPanel.Size = new System.Drawing.Size(85, 32);
            this.descriptionLayoutPanel.TabIndex = 5;
            // 
            // descriptionHeader
            // 
            this.descriptionHeader.AutoSize = true;
            this.descriptionHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionHeader.Location = new System.Drawing.Point(3, 3);
            this.descriptionHeader.Margin = new System.Windows.Forms.Padding(3);
            this.descriptionHeader.Name = "descriptionHeader";
            this.descriptionHeader.Size = new System.Drawing.Size(71, 13);
            this.descriptionHeader.TabIndex = 0;
            this.descriptionHeader.Text = "Description";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(3, 19);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(79, 13);
            this.descriptionLabel.TabIndex = 1;
            this.descriptionLabel.Text = "descriptionText";
            // 
            // createdByLayoutPanel
            // 
            this.createdByLayoutPanel.AutoSize = true;
            this.createdByLayoutPanel.Controls.Add(this.createdByHeader);
            this.createdByLayoutPanel.Controls.Add(this.createdByLabel);
            this.createdByLayoutPanel.Location = new System.Drawing.Point(3, 256);
            this.createdByLayoutPanel.Name = "createdByLayoutPanel";
            this.createdByLayoutPanel.Size = new System.Drawing.Size(160, 13);
            this.createdByLayoutPanel.TabIndex = 6;
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
            this.createdByLabel.Size = new System.Drawing.Size(76, 13);
            this.createdByLabel.TabIndex = 1;
            this.createdByLabel.Text = "createdByText";
            // 
            // lastPublishedLayoutPanel
            // 
            this.lastPublishedLayoutPanel.AutoSize = true;
            this.lastPublishedLayoutPanel.Controls.Add(this.lastPublishedHeader);
            this.lastPublishedLayoutPanel.Controls.Add(this.lastPublishedLabel);
            this.lastPublishedLayoutPanel.Location = new System.Drawing.Point(3, 275);
            this.lastPublishedLayoutPanel.Name = "lastPublishedLayoutPanel";
            this.lastPublishedLayoutPanel.Size = new System.Drawing.Size(196, 13);
            this.lastPublishedLayoutPanel.TabIndex = 7;
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
            this.lastPublishedLabel.Size = new System.Drawing.Size(90, 13);
            this.lastPublishedLabel.TabIndex = 1;
            this.lastPublishedLabel.Text = "lastPublishedText";
            // 
            // downloadsLayoutPanel
            // 
            this.downloadsLayoutPanel.AutoSize = true;
            this.downloadsLayoutPanel.Controls.Add(this.downloadsHeader);
            this.downloadsLayoutPanel.Controls.Add(this.downloadsLabel);
            this.downloadsLayoutPanel.Location = new System.Drawing.Point(3, 294);
            this.downloadsLayoutPanel.Name = "downloadsLayoutPanel";
            this.downloadsLayoutPanel.Size = new System.Drawing.Size(166, 13);
            this.downloadsLayoutPanel.TabIndex = 8;
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
            this.downloadsLabel.Size = new System.Drawing.Size(81, 13);
            this.downloadsLabel.TabIndex = 1;
            this.downloadsLabel.Text = "downloadCount";
            // 
            // detailsLinkLabel
            // 
            this.detailsLinkLabel.AutoSize = true;
            this.detailsLinkLabel.ForeColor = System.Drawing.Color.Blue;
            this.detailsLinkLabel.Image = global::Bonsai.NuGet.Design.Properties.Resources.PackageImage;
            this.detailsLinkLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.detailsLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.detailsLinkLabel.Location = new System.Drawing.Point(6, 310);
            this.detailsLinkLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.detailsLinkLabel.Name = "detailsLinkLabel";
            this.detailsLinkLabel.Size = new System.Drawing.Size(102, 17);
            this.detailsLinkLabel.TabIndex = 9;
            this.detailsLinkLabel.TabStop = true;
            this.detailsLinkLabel.Text = "Package Details";
            this.detailsLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.detailsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.detailsLinkLabel.MouseEnter += new System.EventHandler(this.linkLabel_MouseEnter);
            this.detailsLinkLabel.MouseLeave += new System.EventHandler(this.linkLabel_MouseLeave);
            // 
            // projectLinkLabel
            // 
            this.projectLinkLabel.AutoSize = true;
            this.projectLinkLabel.ForeColor = System.Drawing.Color.Blue;
            this.projectLinkLabel.Image = global::Bonsai.NuGet.Design.Properties.Resources.WebImage;
            this.projectLinkLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.projectLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.projectLinkLabel.Location = new System.Drawing.Point(6, 327);
            this.projectLinkLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.projectLinkLabel.Name = "projectLinkLabel";
            this.projectLinkLabel.Size = new System.Drawing.Size(99, 17);
            this.projectLinkLabel.TabIndex = 10;
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
            this.licenseLinkLabel.Location = new System.Drawing.Point(6, 344);
            this.licenseLinkLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.licenseLinkLabel.Name = "licenseLinkLabel";
            this.licenseLinkLabel.Size = new System.Drawing.Size(82, 17);
            this.licenseLinkLabel.TabIndex = 11;
            this.licenseLinkLabel.TabStop = true;
            this.licenseLinkLabel.Text = "License Info";
            this.licenseLinkLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.licenseLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.licenseLinkLabel_LinkClicked);
            this.licenseLinkLabel.MouseEnter += new System.EventHandler(this.linkLabel_MouseEnter);
            this.licenseLinkLabel.MouseLeave += new System.EventHandler(this.linkLabel_MouseLeave);
            // 
            // reportAbuseLinkLabel
            // 
            this.reportAbuseLinkLabel.AutoSize = true;
            this.reportAbuseLinkLabel.ForeColor = System.Drawing.Color.Blue;
            this.reportAbuseLinkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.reportAbuseLinkLabel.Location = new System.Drawing.Point(6, 364);
            this.reportAbuseLinkLabel.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.reportAbuseLinkLabel.Name = "reportAbuseLinkLabel";
            this.reportAbuseLinkLabel.Size = new System.Drawing.Size(72, 13);
            this.reportAbuseLinkLabel.TabIndex = 12;
            this.reportAbuseLinkLabel.TabStop = true;
            this.reportAbuseLinkLabel.Text = "Report Abuse";
            this.reportAbuseLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel_LinkClicked);
            this.reportAbuseLinkLabel.MouseEnter += new System.EventHandler(this.linkLabel_MouseEnter);
            this.reportAbuseLinkLabel.MouseLeave += new System.EventHandler(this.linkLabel_MouseLeave);
            // 
            // tagsLayoutPanel
            // 
            this.tagsLayoutPanel.AutoSize = true;
            this.tagsLayoutPanel.Controls.Add(this.tagsHeader);
            this.tagsLayoutPanel.Controls.Add(this.tagsLabel);
            this.tagsLayoutPanel.Location = new System.Drawing.Point(3, 383);
            this.tagsLayoutPanel.Name = "tagsLayoutPanel";
            this.tagsLayoutPanel.Size = new System.Drawing.Size(99, 13);
            this.tagsLayoutPanel.TabIndex = 13;
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
            this.tagsLabel.Size = new System.Drawing.Size(48, 13);
            this.tagsLabel.TabIndex = 1;
            this.tagsLabel.Text = "tagsText";
            // 
            // dependenciesHeaderLayoutPanel
            // 
            this.dependenciesHeaderLayoutPanel.AutoSize = true;
            this.dependenciesHeaderLayoutPanel.Controls.Add(this.dependenciesHeader);
            this.dependenciesHeaderLayoutPanel.Location = new System.Drawing.Point(3, 402);
            this.dependenciesHeaderLayoutPanel.Name = "dependenciesHeaderLayoutPanel";
            this.dependenciesHeaderLayoutPanel.Size = new System.Drawing.Size(94, 13);
            this.dependenciesHeaderLayoutPanel.TabIndex = 14;
            // 
            // dependenciesHeader
            // 
            this.dependenciesHeader.AutoSize = true;
            this.dependenciesHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dependenciesHeader.Location = new System.Drawing.Point(3, 0);
            this.dependenciesHeader.Name = "dependenciesHeader";
            this.dependenciesHeader.Size = new System.Drawing.Size(88, 13);
            this.dependenciesHeader.TabIndex = 0;
            this.dependenciesHeader.Text = "Dependencies";
            // 
            // dependenciesLayoutPanel
            // 
            this.dependenciesLayoutPanel.AutoSize = true;
            this.dependenciesLayoutPanel.Controls.Add(this.dependenciesTextBox);
            this.dependenciesLayoutPanel.Location = new System.Drawing.Point(3, 421);
            this.dependenciesLayoutPanel.Name = "dependenciesLayoutPanel";
            this.dependenciesLayoutPanel.Size = new System.Drawing.Size(188, 26);
            this.dependenciesLayoutPanel.TabIndex = 15;
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
            this.dependenciesTextBox.TabStop = false;
            this.dependenciesTextBox.TextChanged += new System.EventHandler(this.dependenciesTextBox_TextChanged);
            // 
            // dependencyWarningLabel
            // 
            this.dependencyWarningLabel.AutoSize = true;
            this.dependencyWarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dependencyWarningLabel.Location = new System.Drawing.Point(3, 450);
            this.dependencyWarningLabel.Name = "dependencyWarningLabel";
            this.dependencyWarningLabel.Size = new System.Drawing.Size(0, 13);
            this.dependencyWarningLabel.TabIndex = 16;
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
            this.packageIdPanel.ResumeLayout(false);
            this.packageIdPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.prefixReservedIcon)).EndInit();
            this.installedVersionLayoutPanel.ResumeLayout(false);
            this.installedVersionLayoutPanel.PerformLayout();
            this.versionLayoutPanel.ResumeLayout(false);
            this.deprecationMetadataPanel.ResumeLayout(false);
            this.deprecationMetadataPanel.PerformLayout();
            this.alternatePackagePanel.ResumeLayout(false);
            this.alternatePackagePanel.PerformLayout();
            this.descriptionLayoutPanel.ResumeLayout(false);
            this.descriptionLayoutPanel.PerformLayout();
            this.createdByLayoutPanel.ResumeLayout(false);
            this.createdByLayoutPanel.PerformLayout();
            this.lastPublishedLayoutPanel.ResumeLayout(false);
            this.lastPublishedLayoutPanel.PerformLayout();
            this.downloadsLayoutPanel.ResumeLayout(false);
            this.downloadsLayoutPanel.PerformLayout();
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
        private System.Windows.Forms.FlowLayoutPanel downloadsLayoutPanel;
        private System.Windows.Forms.Label downloadsHeader;
        private System.Windows.Forms.Label downloadsLabel;
        private System.Windows.Forms.FlowLayoutPanel dependenciesHeaderLayoutPanel;
        private System.Windows.Forms.Label dependenciesHeader;
        private System.Windows.Forms.FlowLayoutPanel dependenciesLayoutPanel;
        private System.Windows.Forms.TextBox dependenciesTextBox;
        private System.Windows.Forms.Label dependencyWarningLabel;
        private System.Windows.Forms.Label deprecationMetadataLabel;
        private ImageLabel warningImageLabel;
        private ImageLinkLabel projectLinkLabel;
        private ImageLinkLabel licenseLinkLabel;
        private ImageLabel packageIdLabel;
        private ImageLinkLabel detailsLinkLabel;
        private System.Windows.Forms.FlowLayoutPanel versionLayoutPanel;
        private System.Windows.Forms.Label versionHeader;
        private System.Windows.Forms.Button operationButton;
        private System.Windows.Forms.ComboBox versionComboBox;
        private System.Windows.Forms.FlowLayoutPanel installedVersionLayoutPanel;
        private System.Windows.Forms.Label installedHeader;
        private System.Windows.Forms.Button uninstallButton;
        private System.Windows.Forms.TextBox installedVersionTextBox;
        private System.Windows.Forms.FlowLayoutPanel deprecationMetadataPanel;
        private System.Windows.Forms.FlowLayoutPanel descriptionLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel alternatePackagePanel;
        private System.Windows.Forms.Label alternatePackageHeader;
        private System.Windows.Forms.LinkLabel alternatePackageLinkLabel;
        private System.Windows.Forms.FlowLayoutPanel packageIdPanel;
        private System.Windows.Forms.PictureBox prefixReservedIcon;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
