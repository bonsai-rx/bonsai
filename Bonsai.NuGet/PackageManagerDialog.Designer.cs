namespace Bonsai.NuGet
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
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.packageViewLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.filterLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.refreshButton = new System.Windows.Forms.Button();
            this.prereleaseCheckBox = new System.Windows.Forms.CheckBox();
            this.pageSelectorPanel = new System.Windows.Forms.Panel();
            this.packageViewPanel = new System.Windows.Forms.TableLayoutPanel();
            this.packageIcons = new System.Windows.Forms.ImageList(this.components);
            this.multiOperationPanel = new System.Windows.Forms.Panel();
            this.multiOperationButton = new System.Windows.Forms.Button();
            this.multiOperationLabel = new System.Windows.Forms.Label();
            this.detailsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.searchLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.packageSourceLabel = new System.Windows.Forms.Label();
            this.packageSourceComboBox = new System.Windows.Forms.ComboBox();
            this.settingsButton = new System.Windows.Forms.Button();
            this.closePanel = new System.Windows.Forms.Panel();
            this.closeButton = new System.Windows.Forms.Button();
            this.operationLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.browseButton = new System.Windows.Forms.RadioButton();
            this.installedButton = new System.Windows.Forms.RadioButton();
            this.updatesButton = new System.Windows.Forms.RadioButton();
            this.searchComboBox = new Bonsai.NuGet.CueBannerComboBox();
            this.packagePageSelector = new Bonsai.NuGet.PackagePageSelector();
            this.packageView = new Bonsai.NuGet.PackageView();
            this.packageDetails = new Bonsai.NuGet.PackageDetails();
            this.saveFolderDialog = new Bonsai.NuGet.SaveFolderDialog();
            this.mainLayoutPanel.SuspendLayout();
            this.packageViewLayoutPanel.SuspendLayout();
            this.filterLayoutPanel.SuspendLayout();
            this.pageSelectorPanel.SuspendLayout();
            this.packageViewPanel.SuspendLayout();
            this.multiOperationPanel.SuspendLayout();
            this.detailsLayoutPanel.SuspendLayout();
            this.searchLayoutPanel.SuspendLayout();
            this.closePanel.SuspendLayout();
            this.operationLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 2;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 337F));
            this.mainLayoutPanel.Controls.Add(this.packageViewLayoutPanel, 0, 1);
            this.mainLayoutPanel.Controls.Add(this.detailsLayoutPanel, 1, 1);
            this.mainLayoutPanel.Controls.Add(this.closePanel, 1, 2);
            this.mainLayoutPanel.Controls.Add(this.operationLayoutPanel, 0, 0);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 3;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 62F));
            this.mainLayoutPanel.Size = new System.Drawing.Size(1072, 672);
            this.mainLayoutPanel.TabIndex = 0;
            // 
            // packageViewLayoutPanel
            // 
            this.packageViewLayoutPanel.ColumnCount = 1;
            this.packageViewLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewLayoutPanel.Controls.Add(this.filterLayoutPanel, 0, 0);
            this.packageViewLayoutPanel.Controls.Add(this.pageSelectorPanel, 0, 2);
            this.packageViewLayoutPanel.Controls.Add(this.packageViewPanel, 0, 1);
            this.packageViewLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageViewLayoutPanel.Location = new System.Drawing.Point(4, 44);
            this.packageViewLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.packageViewLayoutPanel.Name = "packageViewLayoutPanel";
            this.packageViewLayoutPanel.RowCount = 3;
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.packageViewLayoutPanel.Size = new System.Drawing.Size(727, 562);
            this.packageViewLayoutPanel.TabIndex = 2;
            // 
            // filterLayoutPanel
            // 
            this.filterLayoutPanel.Controls.Add(this.searchComboBox);
            this.filterLayoutPanel.Controls.Add(this.refreshButton);
            this.filterLayoutPanel.Controls.Add(this.prereleaseCheckBox);
            this.filterLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterLayoutPanel.Location = new System.Drawing.Point(4, 4);
            this.filterLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.filterLayoutPanel.Name = "filterLayoutPanel";
            this.filterLayoutPanel.Size = new System.Drawing.Size(719, 29);
            this.filterLayoutPanel.TabIndex = 1;
            // 
            // refreshButton
            // 
            this.refreshButton.FlatAppearance.BorderSize = 0;
            this.refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refreshButton.Image = global::Bonsai.NuGet.Properties.Resources.RefreshImage;
            this.refreshButton.Location = new System.Drawing.Point(311, 3);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(24, 23);
            this.refreshButton.TabIndex = 3;
            this.refreshButton.TabStop = false;
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // prereleaseCheckBox
            // 
            this.prereleaseCheckBox.Location = new System.Drawing.Point(341, 3);
            this.prereleaseCheckBox.Name = "prereleaseCheckBox";
            this.prereleaseCheckBox.Size = new System.Drawing.Size(147, 26);
            this.prereleaseCheckBox.TabIndex = 4;
            this.prereleaseCheckBox.Text = "Include prerelease";
            this.prereleaseCheckBox.UseVisualStyleBackColor = true;
            // 
            // pageSelectorPanel
            // 
            this.pageSelectorPanel.Controls.Add(this.packagePageSelector);
            this.pageSelectorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pageSelectorPanel.Location = new System.Drawing.Point(4, 517);
            this.pageSelectorPanel.Margin = new System.Windows.Forms.Padding(4);
            this.pageSelectorPanel.Name = "pageSelectorPanel";
            this.pageSelectorPanel.Size = new System.Drawing.Size(719, 41);
            this.pageSelectorPanel.TabIndex = 2;
            // 
            // packageViewPanel
            // 
            this.packageViewPanel.BackColor = System.Drawing.SystemColors.Control;
            this.packageViewPanel.ColumnCount = 1;
            this.packageViewPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewPanel.Controls.Add(this.packageView, 0, 1);
            this.packageViewPanel.Controls.Add(this.multiOperationPanel, 0, 0);
            this.packageViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageViewPanel.Location = new System.Drawing.Point(4, 41);
            this.packageViewPanel.Margin = new System.Windows.Forms.Padding(4);
            this.packageViewPanel.Name = "packageViewPanel";
            this.packageViewPanel.RowCount = 2;
            this.packageViewPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.packageViewPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewPanel.Size = new System.Drawing.Size(719, 468);
            this.packageViewPanel.TabIndex = 3;
            // 
            // packageIcons
            // 
            this.packageIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.packageIcons.ImageSize = new System.Drawing.Size(32, 32);
            this.packageIcons.TransparentColor = System.Drawing.Color.Violet;
            // 
            // multiOperationPanel
            // 
            this.multiOperationPanel.AutoSize = true;
            this.multiOperationPanel.BackColor = System.Drawing.SystemColors.Control;
            this.multiOperationPanel.Controls.Add(this.multiOperationButton);
            this.multiOperationPanel.Controls.Add(this.multiOperationLabel);
            this.multiOperationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multiOperationPanel.Location = new System.Drawing.Point(0, 0);
            this.multiOperationPanel.Margin = new System.Windows.Forms.Padding(0);
            this.multiOperationPanel.Name = "multiOperationPanel";
            this.multiOperationPanel.Size = new System.Drawing.Size(719, 36);
            this.multiOperationPanel.TabIndex = 0;
            // 
            // multiOperationButton
            // 
            this.multiOperationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.multiOperationButton.Location = new System.Drawing.Point(579, 4);
            this.multiOperationButton.Margin = new System.Windows.Forms.Padding(4);
            this.multiOperationButton.Name = "multiOperationButton";
            this.multiOperationButton.Size = new System.Drawing.Size(100, 28);
            this.multiOperationButton.TabIndex = 1;
            this.multiOperationButton.Text = "Operation";
            this.multiOperationButton.UseVisualStyleBackColor = true;
            this.multiOperationButton.Click += new System.EventHandler(this.multiOperationButton_Click);
            // 
            // multiOperationLabel
            // 
            this.multiOperationLabel.AutoSize = true;
            this.multiOperationLabel.Location = new System.Drawing.Point(4, 10);
            this.multiOperationLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.multiOperationLabel.Name = "multiOperationLabel";
            this.multiOperationLabel.Size = new System.Drawing.Size(106, 17);
            this.multiOperationLabel.TabIndex = 2;
            this.multiOperationLabel.Text = "OperationLabel";
            // 
            // detailsLayoutPanel
            // 
            this.detailsLayoutPanel.ColumnCount = 1;
            this.detailsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailsLayoutPanel.Controls.Add(this.searchLayoutPanel, 0, 0);
            this.detailsLayoutPanel.Controls.Add(this.packageDetails, 0, 1);
            this.detailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsLayoutPanel.Location = new System.Drawing.Point(739, 44);
            this.detailsLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.detailsLayoutPanel.Name = "detailsLayoutPanel";
            this.detailsLayoutPanel.RowCount = 2;
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailsLayoutPanel.Size = new System.Drawing.Size(329, 562);
            this.detailsLayoutPanel.TabIndex = 3;
            // 
            // searchLayoutPanel
            // 
            this.searchLayoutPanel.Controls.Add(this.packageSourceLabel);
            this.searchLayoutPanel.Controls.Add(this.packageSourceComboBox);
            this.searchLayoutPanel.Controls.Add(this.settingsButton);
            this.searchLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchLayoutPanel.Location = new System.Drawing.Point(4, 4);
            this.searchLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.searchLayoutPanel.Name = "searchLayoutPanel";
            this.searchLayoutPanel.Size = new System.Drawing.Size(321, 29);
            this.searchLayoutPanel.TabIndex = 0;
            // 
            // packageSourceLabel
            // 
            this.packageSourceLabel.Location = new System.Drawing.Point(4, 0);
            this.packageSourceLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.packageSourceLabel.Name = "packageSourceLabel";
            this.packageSourceLabel.Size = new System.Drawing.Size(120, 27);
            this.packageSourceLabel.TabIndex = 3;
            this.packageSourceLabel.Text = "Package source:";
            this.packageSourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // packageSourceComboBox
            // 
            this.packageSourceComboBox.DisplayMember = "Key";
            this.packageSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.packageSourceComboBox.FormattingEnabled = true;
            this.packageSourceComboBox.Location = new System.Drawing.Point(132, 4);
            this.packageSourceComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.packageSourceComboBox.Name = "packageSourceComboBox";
            this.packageSourceComboBox.Size = new System.Drawing.Size(150, 24);
            this.packageSourceComboBox.TabIndex = 5;
            this.packageSourceComboBox.SelectedIndexChanged += new System.EventHandler(this.refreshButton_Click);
            // 
            // settingsButton
            // 
            this.settingsButton.FlatAppearance.BorderSize = 0;
            this.settingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsButton.Image = global::Bonsai.NuGet.Properties.Resources.SettingsImage;
            this.settingsButton.Location = new System.Drawing.Point(289, 3);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(24, 23);
            this.settingsButton.TabIndex = 6;
            this.settingsButton.UseVisualStyleBackColor = true;
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // closePanel
            // 
            this.closePanel.Controls.Add(this.closeButton);
            this.closePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.closePanel.Location = new System.Drawing.Point(739, 614);
            this.closePanel.Margin = new System.Windows.Forms.Padding(4);
            this.closePanel.Name = "closePanel";
            this.closePanel.Size = new System.Drawing.Size(329, 54);
            this.closePanel.TabIndex = 5;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(217, 15);
            this.closeButton.Margin = new System.Windows.Forms.Padding(4);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(100, 28);
            this.closeButton.TabIndex = 6;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // operationLayoutPanel
            // 
            this.operationLayoutPanel.Controls.Add(this.browseButton);
            this.operationLayoutPanel.Controls.Add(this.installedButton);
            this.operationLayoutPanel.Controls.Add(this.updatesButton);
            this.operationLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.operationLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.operationLayoutPanel.Name = "operationLayoutPanel";
            this.operationLayoutPanel.Size = new System.Drawing.Size(729, 34);
            this.operationLayoutPanel.TabIndex = 0;
            this.operationLayoutPanel.TabStop = true;
            // 
            // browseButton
            // 
            this.browseButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.browseButton.AutoSize = true;
            this.browseButton.FlatAppearance.BorderSize = 0;
            this.browseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.browseButton.Location = new System.Drawing.Point(3, 3);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(64, 27);
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
            this.installedButton.Location = new System.Drawing.Point(73, 3);
            this.installedButton.Name = "installedButton";
            this.installedButton.Size = new System.Drawing.Size(70, 27);
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
            this.updatesButton.Location = new System.Drawing.Point(149, 3);
            this.updatesButton.Name = "updatesButton";
            this.updatesButton.Size = new System.Drawing.Size(71, 27);
            this.updatesButton.TabIndex = 2;
            this.updatesButton.Text = "Updates";
            this.updatesButton.UseVisualStyleBackColor = true;
            this.updatesButton.CheckedChanged += new System.EventHandler(this.refreshButton_Click);
            // 
            // searchComboBox
            // 
            this.searchComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.searchComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.searchComboBox.CueBanner = null;
            this.searchComboBox.FormattingEnabled = true;
            this.searchComboBox.Location = new System.Drawing.Point(4, 4);
            this.searchComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.searchComboBox.Name = "searchComboBox";
            this.searchComboBox.Size = new System.Drawing.Size(300, 24);
            this.searchComboBox.TabIndex = 1;
            // 
            // packagePageSelector
            // 
            this.packagePageSelector.AutoSize = true;
            this.packagePageSelector.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packagePageSelector.Location = new System.Drawing.Point(0, 0);
            this.packagePageSelector.Margin = new System.Windows.Forms.Padding(5);
            this.packagePageSelector.Name = "packagePageSelector";
            this.packagePageSelector.SelectedPage = -1;
            this.packagePageSelector.Size = new System.Drawing.Size(719, 41);
            this.packagePageSelector.TabIndex = 3;
            // 
            // packageView
            // 
            this.packageView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this.packageView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.packageView.CanSelectNodes = false;
            this.packageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.packageView.FullRowSelect = true;
            this.packageView.ImageIndex = 0;
            this.packageView.ImageList = this.packageIcons;
            this.packageView.ItemHeight = 64;
            this.packageView.Location = new System.Drawing.Point(4, 40);
            this.packageView.Margin = new System.Windows.Forms.Padding(4);
            this.packageView.Name = "packageView";
            this.packageView.OperationText = null;
            this.packageView.SelectedImageIndex = 0;
            this.packageView.ShowLines = false;
            this.packageView.ShowRootLines = false;
            this.packageView.Size = new System.Drawing.Size(711, 424);
            this.packageView.TabIndex = 2;
            this.packageView.OperationClick += new System.Windows.Forms.TreeViewEventHandler(this.packageView_OperationClick);
            // 
            // packageDetails
            // 
            this.packageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageDetails.Location = new System.Drawing.Point(5, 42);
            this.packageDetails.Margin = new System.Windows.Forms.Padding(5);
            this.packageDetails.Name = "packageDetails";
            this.packageDetails.Size = new System.Drawing.Size(319, 515);
            this.packageDetails.TabIndex = 1;
            // 
            // saveFolderDialog
            // 
            this.saveFolderDialog.FileName = "";
            // 
            // PackageManagerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(1072, 672);
            this.Controls.Add(this.mainLayoutPanel);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1087, 709);
            this.Name = "PackageManagerDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bonsai - Manage Packages";
            this.mainLayoutPanel.ResumeLayout(false);
            this.packageViewLayoutPanel.ResumeLayout(false);
            this.filterLayoutPanel.ResumeLayout(false);
            this.pageSelectorPanel.ResumeLayout(false);
            this.pageSelectorPanel.PerformLayout();
            this.packageViewPanel.ResumeLayout(false);
            this.packageViewPanel.PerformLayout();
            this.multiOperationPanel.ResumeLayout(false);
            this.multiOperationPanel.PerformLayout();
            this.detailsLayoutPanel.ResumeLayout(false);
            this.searchLayoutPanel.ResumeLayout(false);
            this.closePanel.ResumeLayout(false);
            this.operationLayoutPanel.ResumeLayout(false);
            this.operationLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.ImageList packageIcons;
        private System.Windows.Forms.FlowLayoutPanel filterLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel packageViewLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel detailsLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel searchLayoutPanel;
        private Bonsai.NuGet.CueBannerComboBox searchComboBox;
        private PackageDetails packageDetails;
        private System.Windows.Forms.Panel pageSelectorPanel;
        private PackagePageSelector packagePageSelector;
        private System.Windows.Forms.Panel closePanel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.TableLayoutPanel packageViewPanel;
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
    }
}