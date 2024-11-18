namespace Bonsai.NuGet.Design
{
    partial class PackageManagerDialog
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
            this.components = new System.ComponentModel.Container();
            this.packageIcons = new System.Windows.Forms.ImageList(this.components);
            this.closeButton = new System.Windows.Forms.Button();
            this.mainLayoutPanel = new Bonsai.NuGet.Design.TableLayoutPanel();
            this.operationLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.browseButton = new System.Windows.Forms.RadioButton();
            this.installedButton = new System.Windows.Forms.RadioButton();
            this.updatesButton = new System.Windows.Forms.RadioButton();
            this.packageViewLayoutPanel = new Bonsai.NuGet.Design.TableLayoutPanel();
            this.filterLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.searchComboBox = new Bonsai.NuGet.Design.CueBannerComboBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.prereleaseCheckBox = new System.Windows.Forms.CheckBox();
            this.dependencyCheckBox = new System.Windows.Forms.CheckBox();
            this.packageViewPanel = new Bonsai.NuGet.Design.TableLayoutPanel();
            this.multiOperationPanel = new System.Windows.Forms.Panel();
            this.multiOperationLabel = new System.Windows.Forms.Label();
            this.multiOperationButton = new System.Windows.Forms.Button();
            this.packageView = new Bonsai.NuGet.Design.PackageView();
            this.pageSelectorPanel = new System.Windows.Forms.Panel();
            this.packagePageSelector = new Bonsai.NuGet.Design.PackagePageSelector();
            this.detailsLayoutPanel = new Bonsai.NuGet.Design.TableLayoutPanel();
            this.searchLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.settingsButton = new System.Windows.Forms.Button();
            this.packageSourceComboBox = new System.Windows.Forms.ComboBox();
            this.packageSourceLabel = new System.Windows.Forms.Label();
            this.packageDetails = new Bonsai.NuGet.Design.PackageDetails();
            this.closePanel = new System.Windows.Forms.Panel();
            this.saveFolderDialog = new Bonsai.NuGet.Design.SaveFolderDialog();
            this.mainLayoutPanel.SuspendLayout();
            this.operationLayoutPanel.SuspendLayout();
            this.packageViewLayoutPanel.SuspendLayout();
            this.filterLayoutPanel.SuspendLayout();
            this.packageViewPanel.SuspendLayout();
            this.multiOperationPanel.SuspendLayout();
            this.pageSelectorPanel.SuspendLayout();
            this.detailsLayoutPanel.SuspendLayout();
            this.searchLayoutPanel.SuspendLayout();
            this.closePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // packageIcons
            // 
            this.packageIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.packageIcons.ImageSize = new System.Drawing.Size(32, 32);
            this.packageIcons.TransparentColor = System.Drawing.Color.Violet;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(210, 12);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 6;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 2;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.mainLayoutPanel.Controls.Add(this.operationLayoutPanel, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.packageViewLayoutPanel, 0, 1);
            this.mainLayoutPanel.Controls.Add(this.detailsLayoutPanel, 1, 1);
            this.mainLayoutPanel.Controls.Add(this.closePanel, 1, 2);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 3;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainLayoutPanel.Size = new System.Drawing.Size(944, 546);
            this.mainLayoutPanel.TabIndex = 0;
            // 
            // operationLayoutPanel
            // 
            this.operationLayoutPanel.Controls.Add(this.browseButton);
            this.operationLayoutPanel.Controls.Add(this.installedButton);
            this.operationLayoutPanel.Controls.Add(this.updatesButton);
            this.operationLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.operationLayoutPanel.Location = new System.Drawing.Point(2, 2);
            this.operationLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
            this.operationLayoutPanel.Name = "operationLayoutPanel";
            this.operationLayoutPanel.Size = new System.Drawing.Size(640, 28);
            this.operationLayoutPanel.TabIndex = 0;
            this.operationLayoutPanel.TabStop = true;
            // 
            // browseButton
            // 
            this.browseButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.browseButton.AutoSize = true;
            this.browseButton.FlatAppearance.BorderSize = 0;
            this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.browseButton.Location = new System.Drawing.Point(2, 2);
            this.browseButton.Margin = new System.Windows.Forms.Padding(2);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(52, 23);
            this.browseButton.TabIndex = 0;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.CheckedChanged += new System.EventHandler(this.refreshButton_Click);
            // 
            // installedButton
            // 
            this.installedButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.installedButton.AutoSize = true;
            this.installedButton.FlatAppearance.BorderSize = 0;
            this.installedButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.installedButton.Location = new System.Drawing.Point(58, 2);
            this.installedButton.Margin = new System.Windows.Forms.Padding(2);
            this.installedButton.Name = "installedButton";
            this.installedButton.Size = new System.Drawing.Size(56, 23);
            this.installedButton.TabIndex = 1;
            this.installedButton.Text = "Installed";
            this.installedButton.UseVisualStyleBackColor = true;
            this.installedButton.CheckedChanged += new System.EventHandler(this.refreshButton_Click);
            // 
            // updatesButton
            // 
            this.updatesButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.updatesButton.AutoSize = true;
            this.updatesButton.FlatAppearance.BorderSize = 0;
            this.updatesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.updatesButton.Location = new System.Drawing.Point(118, 2);
            this.updatesButton.Margin = new System.Windows.Forms.Padding(2);
            this.updatesButton.Name = "updatesButton";
            this.updatesButton.Size = new System.Drawing.Size(57, 23);
            this.updatesButton.TabIndex = 2;
            this.updatesButton.Text = "Updates";
            this.updatesButton.UseVisualStyleBackColor = true;
            this.updatesButton.CheckedChanged += new System.EventHandler(this.refreshButton_Click);
            // 
            // packageViewLayoutPanel
            // 
            this.packageViewLayoutPanel.ColumnCount = 1;
            this.packageViewLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewLayoutPanel.Controls.Add(this.filterLayoutPanel, 0, 0);
            this.packageViewLayoutPanel.Controls.Add(this.packageViewPanel, 0, 1);
            this.packageViewLayoutPanel.Controls.Add(this.pageSelectorPanel, 0, 2);
            this.packageViewLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageViewLayoutPanel.Location = new System.Drawing.Point(3, 35);
            this.packageViewLayoutPanel.Name = "packageViewLayoutPanel";
            this.packageViewLayoutPanel.RowCount = 3;
            this.mainLayoutPanel.SetRowSpan(this.packageViewLayoutPanel, 2);
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.packageViewLayoutPanel.Size = new System.Drawing.Size(638, 508);
            this.packageViewLayoutPanel.TabIndex = 1;
            // 
            // filterLayoutPanel
            // 
            this.filterLayoutPanel.Controls.Add(this.searchComboBox);
            this.filterLayoutPanel.Controls.Add(this.refreshButton);
            this.filterLayoutPanel.Controls.Add(this.prereleaseCheckBox);
            this.filterLayoutPanel.Controls.Add(this.dependencyCheckBox);
            this.filterLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.filterLayoutPanel.Name = "filterLayoutPanel";
            this.filterLayoutPanel.Size = new System.Drawing.Size(632, 24);
            this.filterLayoutPanel.TabIndex = 0;
            // 
            // searchComboBox
            // 
            this.searchComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.searchComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.searchComboBox.CueBanner = null;
            this.searchComboBox.FormattingEnabled = true;
            this.searchComboBox.Location = new System.Drawing.Point(3, 3);
            this.searchComboBox.Name = "searchComboBox";
            this.searchComboBox.Size = new System.Drawing.Size(226, 21);
            this.searchComboBox.TabIndex = 0;
            // 
            // refreshButton
            // 
            this.refreshButton.FlatAppearance.BorderSize = 0;
            this.refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshButton.Image = global::Bonsai.NuGet.Design.Properties.Resources.RefreshImage;
            this.refreshButton.Location = new System.Drawing.Point(234, 2);
            this.refreshButton.Margin = new System.Windows.Forms.Padding(2);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(18, 19);
            this.refreshButton.TabIndex = 1;
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // prereleaseCheckBox
            // 
            this.prereleaseCheckBox.AutoSize = true;
            this.prereleaseCheckBox.Location = new System.Drawing.Point(256, 5);
            this.prereleaseCheckBox.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.prereleaseCheckBox.Name = "prereleaseCheckBox";
            this.prereleaseCheckBox.Size = new System.Drawing.Size(113, 17);
            this.prereleaseCheckBox.TabIndex = 2;
            this.prereleaseCheckBox.Text = "Include prerelease";
            this.prereleaseCheckBox.UseVisualStyleBackColor = true;
            // 
            // dependencyCheckBox
            // 
            this.dependencyCheckBox.AutoSize = true;
            this.dependencyCheckBox.Location = new System.Drawing.Point(373, 5);
            this.dependencyCheckBox.Margin = new System.Windows.Forms.Padding(2, 5, 2, 2);
            this.dependencyCheckBox.Name = "dependencyCheckBox";
            this.dependencyCheckBox.Size = new System.Drawing.Size(123, 17);
            this.dependencyCheckBox.TabIndex = 3;
            this.dependencyCheckBox.Text = "Show dependencies";
            this.dependencyCheckBox.UseVisualStyleBackColor = true;
            this.dependencyCheckBox.CheckedChanged += new System.EventHandler(this.dependencyCheckBox_CheckedChanged);
            // 
            // packageViewPanel
            // 
            this.packageViewPanel.BackColor = System.Drawing.SystemColors.Control;
            this.packageViewPanel.ColumnCount = 1;
            this.packageViewPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewPanel.Controls.Add(this.multiOperationPanel, 0, 0);
            this.packageViewPanel.Controls.Add(this.packageView, 0, 1);
            this.packageViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageViewPanel.Location = new System.Drawing.Point(3, 33);
            this.packageViewPanel.Name = "packageViewPanel";
            this.packageViewPanel.RowCount = 2;
            this.packageViewPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.packageViewPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewPanel.Size = new System.Drawing.Size(632, 426);
            this.packageViewPanel.TabIndex = 1;
            // 
            // multiOperationPanel
            // 
            this.multiOperationPanel.AutoSize = true;
            this.multiOperationPanel.BackColor = System.Drawing.SystemColors.Control;
            this.multiOperationPanel.Controls.Add(this.multiOperationLabel);
            this.multiOperationPanel.Controls.Add(this.multiOperationButton);
            this.multiOperationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multiOperationPanel.Location = new System.Drawing.Point(0, 0);
            this.multiOperationPanel.Margin = new System.Windows.Forms.Padding(0);
            this.multiOperationPanel.Name = "multiOperationPanel";
            this.multiOperationPanel.Size = new System.Drawing.Size(632, 29);
            this.multiOperationPanel.TabIndex = 0;
            // 
            // multiOperationLabel
            // 
            this.multiOperationLabel.AutoSize = true;
            this.multiOperationLabel.Location = new System.Drawing.Point(3, 8);
            this.multiOperationLabel.Name = "multiOperationLabel";
            this.multiOperationLabel.Size = new System.Drawing.Size(79, 13);
            this.multiOperationLabel.TabIndex = 0;
            this.multiOperationLabel.Text = "OperationLabel";
            // 
            // multiOperationButton
            // 
            this.multiOperationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.multiOperationButton.Location = new System.Drawing.Point(527, 3);
            this.multiOperationButton.Name = "multiOperationButton";
            this.multiOperationButton.Size = new System.Drawing.Size(75, 23);
            this.multiOperationButton.TabIndex = 1;
            this.multiOperationButton.Text = "Operation";
            this.multiOperationButton.UseVisualStyleBackColor = true;
            this.multiOperationButton.Click += new System.EventHandler(this.multiOperationButton_Click);
            // 
            // packageView
            // 
            this.packageView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this.packageView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.packageView.CanSelectNodes = false;
            this.packageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageView.FullRowSelect = true;
            this.packageView.HotTracking = true;
            this.packageView.ImageIndex = 0;
            this.packageView.ImageList = this.packageIcons;
            this.packageView.ItemHeight = 64;
            this.packageView.Location = new System.Drawing.Point(3, 32);
            this.packageView.Name = "packageView";
            this.packageView.Operation = Bonsai.NuGet.Design.PackageOperationType.Install;
            this.packageView.SelectedImageIndex = 0;
            this.packageView.ShowLines = false;
            this.packageView.ShowRootLines = false;
            this.packageView.Size = new System.Drawing.Size(626, 391);
            this.packageView.TabIndex = 1;
            this.packageView.OperationClick += new Bonsai.NuGet.Design.PackageViewEventHandler(this.packageView_OperationClick);
            // 
            // pageSelectorPanel
            // 
            this.pageSelectorPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pageSelectorPanel.Controls.Add(this.packagePageSelector);
            this.pageSelectorPanel.Location = new System.Drawing.Point(3, 465);
            this.pageSelectorPanel.Name = "pageSelectorPanel";
            this.pageSelectorPanel.Size = new System.Drawing.Size(632, 40);
            this.pageSelectorPanel.TabIndex = 2;
            // 
            // packagePageSelector
            // 
            this.packagePageSelector.Location = new System.Drawing.Point(219, 7);
            this.packagePageSelector.Margin = new System.Windows.Forms.Padding(4);
            this.packagePageSelector.Name = "packagePageSelector";
            this.packagePageSelector.SelectedPage = 0;
            this.packagePageSelector.ShowNext = false;
            this.packagePageSelector.Size = new System.Drawing.Size(75, 27);
            this.packagePageSelector.TabIndex = 0;
            // 
            // detailsLayoutPanel
            // 
            this.detailsLayoutPanel.ColumnCount = 1;
            this.detailsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailsLayoutPanel.Controls.Add(this.searchLayoutPanel, 0, 0);
            this.detailsLayoutPanel.Controls.Add(this.packageDetails, 0, 1);
            this.detailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsLayoutPanel.Location = new System.Drawing.Point(647, 35);
            this.detailsLayoutPanel.Name = "detailsLayoutPanel";
            this.detailsLayoutPanel.RowCount = 2;
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailsLayoutPanel.Size = new System.Drawing.Size(294, 458);
            this.detailsLayoutPanel.TabIndex = 2;
            // 
            // searchLayoutPanel
            // 
            this.searchLayoutPanel.Controls.Add(this.settingsButton);
            this.searchLayoutPanel.Controls.Add(this.packageSourceComboBox);
            this.searchLayoutPanel.Controls.Add(this.packageSourceLabel);
            this.searchLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.searchLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.searchLayoutPanel.Name = "searchLayoutPanel";
            this.searchLayoutPanel.Size = new System.Drawing.Size(288, 24);
            this.searchLayoutPanel.TabIndex = 0;
            // 
            // settingsButton
            // 
            this.settingsButton.FlatAppearance.BorderSize = 0;
            this.settingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsButton.Image = global::Bonsai.NuGet.Design.Properties.Resources.SettingsImage;
            this.settingsButton.Location = new System.Drawing.Point(268, 2);
            this.settingsButton.Margin = new System.Windows.Forms.Padding(2);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(18, 19);
            this.settingsButton.TabIndex = 2;
            this.settingsButton.UseVisualStyleBackColor = true;
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // packageSourceComboBox
            // 
            this.packageSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.packageSourceComboBox.FormattingEnabled = true;
            this.packageSourceComboBox.Location = new System.Drawing.Point(149, 3);
            this.packageSourceComboBox.Name = "packageSourceComboBox";
            this.packageSourceComboBox.Size = new System.Drawing.Size(114, 21);
            this.packageSourceComboBox.TabIndex = 1;
            this.packageSourceComboBox.SelectedIndexChanged += new System.EventHandler(this.refreshButton_Click);
            // 
            // packageSourceLabel
            // 
            this.packageSourceLabel.Location = new System.Drawing.Point(56, 0);
            this.packageSourceLabel.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.packageSourceLabel.Name = "packageSourceLabel";
            this.packageSourceLabel.Size = new System.Drawing.Size(90, 22);
            this.packageSourceLabel.TabIndex = 0;
            this.packageSourceLabel.Text = "Package source:";
            this.packageSourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // packageDetails
            // 
            this.packageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageDetails.Location = new System.Drawing.Point(0, 34);
            this.packageDetails.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            this.packageDetails.Name = "packageDetails";
            this.packageDetails.Size = new System.Drawing.Size(290, 420);
            this.packageDetails.TabIndex = 1;
            this.packageDetails.OperationClick += new Bonsai.NuGet.Design.PackageViewEventHandler(this.packageView_OperationClick);
            // 
            // closePanel
            // 
            this.closePanel.Controls.Add(this.closeButton);
            this.closePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.closePanel.Location = new System.Drawing.Point(647, 499);
            this.closePanel.Name = "closePanel";
            this.closePanel.Size = new System.Drawing.Size(294, 44);
            this.closePanel.TabIndex = 3;
            // 
            // saveFolderDialog
            // 
            this.saveFolderDialog.FileName = "";
            // 
            // PackageManagerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(944, 546);
            this.Controls.Add(this.mainLayoutPanel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(850, 583);
            this.Name = "PackageManagerDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bonsai - Manage Packages";
            this.mainLayoutPanel.ResumeLayout(false);
            this.operationLayoutPanel.ResumeLayout(false);
            this.operationLayoutPanel.PerformLayout();
            this.packageViewLayoutPanel.ResumeLayout(false);
            this.filterLayoutPanel.ResumeLayout(false);
            this.filterLayoutPanel.PerformLayout();
            this.packageViewPanel.ResumeLayout(false);
            this.packageViewPanel.PerformLayout();
            this.multiOperationPanel.ResumeLayout(false);
            this.multiOperationPanel.PerformLayout();
            this.pageSelectorPanel.ResumeLayout(false);
            this.detailsLayoutPanel.ResumeLayout(false);
            this.searchLayoutPanel.ResumeLayout(false);
            this.closePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Bonsai.NuGet.Design.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.ImageList packageIcons;
        private System.Windows.Forms.FlowLayoutPanel filterLayoutPanel;
        private Bonsai.NuGet.Design.TableLayoutPanel packageViewLayoutPanel;
        private Bonsai.NuGet.Design.TableLayoutPanel detailsLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel searchLayoutPanel;
        private Bonsai.NuGet.Design.CueBannerComboBox searchComboBox;
        private PackageDetails packageDetails;
        private System.Windows.Forms.Panel pageSelectorPanel;
        private PackagePageSelector packagePageSelector;
        private System.Windows.Forms.Panel closePanel;
        private System.Windows.Forms.Button closeButton;
        private Bonsai.NuGet.Design.TableLayoutPanel packageViewPanel;
        private PackageView packageView;
        private System.Windows.Forms.Panel multiOperationPanel;
        private System.Windows.Forms.Button multiOperationButton;
        private System.Windows.Forms.Label multiOperationLabel;
        private SaveFolderDialog saveFolderDialog;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.CheckBox prereleaseCheckBox;
        private System.Windows.Forms.Label packageSourceLabel;
        private System.Windows.Forms.ComboBox packageSourceComboBox;
        private System.Windows.Forms.FlowLayoutPanel operationLayoutPanel;
        private System.Windows.Forms.RadioButton browseButton;
        private System.Windows.Forms.RadioButton installedButton;
        private System.Windows.Forms.RadioButton updatesButton;
        private System.Windows.Forms.Button settingsButton;
        private System.Windows.Forms.CheckBox dependencyCheckBox;
    }
}
