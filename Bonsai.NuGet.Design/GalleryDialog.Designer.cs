namespace Bonsai.NuGet.Design
{
    partial class GalleryDialog
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
            this.mainLayoutPanel = new Bonsai.NuGet.Design.TableLayoutPanel();
            this.packageViewLayoutPanel = new Bonsai.NuGet.Design.TableLayoutPanel();
            this.filterLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.searchComboBox = new Bonsai.NuGet.Design.CueBannerComboBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.prereleaseCheckBox = new System.Windows.Forms.CheckBox();
            this.pageSelectorPanel = new System.Windows.Forms.Panel();
            this.packagePageSelector = new Bonsai.NuGet.Design.PackagePageSelector();
            this.packageViewPanel = new Bonsai.NuGet.Design.TableLayoutPanel();
            this.packageView = new Bonsai.NuGet.Design.PackageView();
            this.packageIcons = new System.Windows.Forms.ImageList(this.components);
            this.detailsLayoutPanel = new Bonsai.NuGet.Design.TableLayoutPanel();
            this.searchLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.settingsButton = new System.Windows.Forms.Button();
            this.packageSourceComboBox = new System.Windows.Forms.ComboBox();
            this.sortLabel = new System.Windows.Forms.Label();
            this.packageDetails = new Bonsai.NuGet.Design.PackageDetails();
            this.closePanel = new System.Windows.Forms.Panel();
            this.closeButton = new System.Windows.Forms.Button();
            this.saveFolderDialog = new Bonsai.NuGet.Design.SaveFolderDialog();
            this.mainLayoutPanel.SuspendLayout();
            this.packageViewLayoutPanel.SuspendLayout();
            this.filterLayoutPanel.SuspendLayout();
            this.pageSelectorPanel.SuspendLayout();
            this.packageViewPanel.SuspendLayout();
            this.detailsLayoutPanel.SuspendLayout();
            this.searchLayoutPanel.SuspendLayout();
            this.closePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 2;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.mainLayoutPanel.Controls.Add(this.packageViewLayoutPanel, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.detailsLayoutPanel, 1, 0);
            this.mainLayoutPanel.Controls.Add(this.closePanel, 1, 1);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 2;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainLayoutPanel.Size = new System.Drawing.Size(944, 546);
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
            this.packageViewLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.packageViewLayoutPanel.Name = "packageViewLayoutPanel";
            this.packageViewLayoutPanel.RowCount = 3;
            this.mainLayoutPanel.SetRowSpan(this.packageViewLayoutPanel, 2);
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.packageViewLayoutPanel.Size = new System.Drawing.Size(638, 540);
            this.packageViewLayoutPanel.TabIndex = 0;
            // 
            // filterLayoutPanel
            // 
            this.filterLayoutPanel.Controls.Add(this.searchComboBox);
            this.filterLayoutPanel.Controls.Add(this.refreshButton);
            this.filterLayoutPanel.Controls.Add(this.prereleaseCheckBox);
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
            // pageSelectorPanel
            // 
            this.pageSelectorPanel.Controls.Add(this.packagePageSelector);
            this.pageSelectorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pageSelectorPanel.Location = new System.Drawing.Point(3, 497);
            this.pageSelectorPanel.Name = "pageSelectorPanel";
            this.pageSelectorPanel.Size = new System.Drawing.Size(632, 40);
            this.pageSelectorPanel.TabIndex = 1;
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
            // packageViewPanel
            // 
            this.packageViewPanel.BackColor = System.Drawing.SystemColors.Control;
            this.packageViewPanel.ColumnCount = 1;
            this.packageViewPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewPanel.Controls.Add(this.packageView, 0, 1);
            this.packageViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageViewPanel.Location = new System.Drawing.Point(3, 33);
            this.packageViewPanel.Name = "packageViewPanel";
            this.packageViewPanel.RowCount = 2;
            this.packageViewPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.packageViewPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewPanel.Size = new System.Drawing.Size(632, 458);
            this.packageViewPanel.TabIndex = 2;
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
            this.packageView.Location = new System.Drawing.Point(3, 3);
            this.packageView.Name = "packageView";
            this.packageView.Operation = Bonsai.NuGet.Design.PackageOperationType.Open;
            this.packageView.SelectedImageIndex = 0;
            this.packageView.ShowLines = false;
            this.packageView.ShowRootLines = false;
            this.packageView.Size = new System.Drawing.Size(626, 452);
            this.packageView.TabIndex = 0;
            this.packageView.OperationClick += new Bonsai.NuGet.Design.PackageViewEventHandler(this.packageView_OperationClick);
            // 
            // packageIcons
            // 
            this.packageIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.packageIcons.ImageSize = new System.Drawing.Size(32, 32);
            this.packageIcons.TransparentColor = System.Drawing.Color.Violet;
            // 
            // detailsLayoutPanel
            // 
            this.detailsLayoutPanel.ColumnCount = 1;
            this.detailsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailsLayoutPanel.Controls.Add(this.searchLayoutPanel, 0, 0);
            this.detailsLayoutPanel.Controls.Add(this.packageDetails, 0, 1);
            this.detailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsLayoutPanel.Location = new System.Drawing.Point(647, 3);
            this.detailsLayoutPanel.Name = "detailsLayoutPanel";
            this.detailsLayoutPanel.RowCount = 2;
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.detailsLayoutPanel.Size = new System.Drawing.Size(294, 490);
            this.detailsLayoutPanel.TabIndex = 1;
            // 
            // searchLayoutPanel
            // 
            this.searchLayoutPanel.Controls.Add(this.settingsButton);
            this.searchLayoutPanel.Controls.Add(this.packageSourceComboBox);
            this.searchLayoutPanel.Controls.Add(this.sortLabel);
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
            // sortLabel
            // 
            this.sortLabel.Location = new System.Drawing.Point(53, 0);
            this.sortLabel.Name = "sortLabel";
            this.sortLabel.Size = new System.Drawing.Size(90, 22);
            this.sortLabel.TabIndex = 0;
            this.sortLabel.Text = "Package source:";
            this.sortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // packageDetails
            // 
            this.packageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageDetails.Location = new System.Drawing.Point(4, 34);
            this.packageDetails.Margin = new System.Windows.Forms.Padding(4);
            this.packageDetails.Name = "packageDetails";
            this.packageDetails.Size = new System.Drawing.Size(286, 452);
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
            this.closePanel.TabIndex = 2;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(210, 12);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // saveFolderDialog
            // 
            this.saveFolderDialog.FileName = "";
            // 
            // GalleryDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(944, 546);
            this.Controls.Add(this.mainLayoutPanel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(850, 583);
            this.Name = "GalleryDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bonsai - Gallery";
            this.mainLayoutPanel.ResumeLayout(false);
            this.packageViewLayoutPanel.ResumeLayout(false);
            this.filterLayoutPanel.ResumeLayout(false);
            this.filterLayoutPanel.PerformLayout();
            this.pageSelectorPanel.ResumeLayout(false);
            this.packageViewPanel.ResumeLayout(false);
            this.detailsLayoutPanel.ResumeLayout(false);
            this.searchLayoutPanel.ResumeLayout(false);
            this.closePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Bonsai.NuGet.Design.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.ImageList packageIcons;
        private System.Windows.Forms.FlowLayoutPanel filterLayoutPanel;
        private System.Windows.Forms.ComboBox packageSourceComboBox;
        private System.Windows.Forms.Label sortLabel;
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
        private SaveFolderDialog saveFolderDialog;
        private System.Windows.Forms.CheckBox prereleaseCheckBox;
        private System.Windows.Forms.Button settingsButton;
        private System.Windows.Forms.Button refreshButton;
    }
}
