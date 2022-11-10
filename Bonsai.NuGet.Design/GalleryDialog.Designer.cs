﻿namespace Bonsai.NuGet.Design
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
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.packageViewLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.filterLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.refreshButton = new System.Windows.Forms.Button();
            this.prereleaseCheckBox = new System.Windows.Forms.CheckBox();
            this.pageSelectorPanel = new System.Windows.Forms.Panel();
            this.packageViewPanel = new System.Windows.Forms.TableLayoutPanel();
            this.packageIcons = new System.Windows.Forms.ImageList(this.components);
            this.detailsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.searchLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.sortLabel = new System.Windows.Forms.Label();
            this.packageSourceComboBox = new System.Windows.Forms.ComboBox();
            this.settingsButton = new System.Windows.Forms.Button();
            this.closePanel = new System.Windows.Forms.Panel();
            this.closeButton = new System.Windows.Forms.Button();
            this.settingsPanel = new System.Windows.Forms.Panel();
            this.searchComboBox = new Bonsai.NuGet.Design.CueBannerComboBox();
            this.packagePageSelector = new Bonsai.NuGet.Design.PackagePageSelector();
            this.packageView = new Bonsai.NuGet.Design.PackageView();
            this.packageDetails = new Bonsai.NuGet.Design.PackageDetails();
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
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 337F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.mainLayoutPanel.Controls.Add(this.packageViewLayoutPanel, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.detailsLayoutPanel, 1, 0);
            this.mainLayoutPanel.Controls.Add(this.closePanel, 1, 1);
            this.mainLayoutPanel.Controls.Add(this.settingsPanel, 0, 1);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 2;
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
            this.packageViewLayoutPanel.Location = new System.Drawing.Point(4, 4);
            this.packageViewLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.packageViewLayoutPanel.Name = "packageViewLayoutPanel";
            this.packageViewLayoutPanel.RowCount = 3;
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.packageViewLayoutPanel.Size = new System.Drawing.Size(727, 602);
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
            this.refreshButton.Image = global::Bonsai.NuGet.Design.Properties.Resources.RefreshImage;
            this.refreshButton.Location = new System.Drawing.Point(311, 3);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(24, 23);
            this.refreshButton.TabIndex = 5;
            this.refreshButton.TabStop = false;
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // prereleaseCheckBox
            // 
            this.prereleaseCheckBox.AutoSize = true;
            this.prereleaseCheckBox.Location = new System.Drawing.Point(341, 3);
            this.prereleaseCheckBox.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.prereleaseCheckBox.Name = "prereleaseCheckBox";
            this.prereleaseCheckBox.Size = new System.Drawing.Size(147, 26);
            this.prereleaseCheckBox.TabIndex = 3;
            this.prereleaseCheckBox.Text = "Include prerelease";
            this.prereleaseCheckBox.UseVisualStyleBackColor = true;
            // 
            // pageSelectorPanel
            // 
            this.pageSelectorPanel.Controls.Add(this.packagePageSelector);
            this.pageSelectorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pageSelectorPanel.Location = new System.Drawing.Point(4, 557);
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
            this.packageViewPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageViewPanel.Location = new System.Drawing.Point(4, 41);
            this.packageViewPanel.Margin = new System.Windows.Forms.Padding(4);
            this.packageViewPanel.Name = "packageViewPanel";
            this.packageViewPanel.RowCount = 2;
            this.packageViewPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.packageViewPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewPanel.Size = new System.Drawing.Size(719, 508);
            this.packageViewPanel.TabIndex = 3;
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
            this.detailsLayoutPanel.Location = new System.Drawing.Point(739, 4);
            this.detailsLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.detailsLayoutPanel.Name = "detailsLayoutPanel";
            this.detailsLayoutPanel.RowCount = 2;
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailsLayoutPanel.Size = new System.Drawing.Size(329, 602);
            this.detailsLayoutPanel.TabIndex = 3;
            // 
            // searchLayoutPanel
            // 
            this.searchLayoutPanel.Controls.Add(this.sortLabel);
            this.searchLayoutPanel.Controls.Add(this.packageSourceComboBox);
            this.searchLayoutPanel.Controls.Add(this.settingsButton);
            this.searchLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchLayoutPanel.Location = new System.Drawing.Point(4, 4);
            this.searchLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.searchLayoutPanel.Name = "searchLayoutPanel";
            this.searchLayoutPanel.Size = new System.Drawing.Size(321, 29);
            this.searchLayoutPanel.TabIndex = 0;
            // 
            // sortLabel
            // 
            this.sortLabel.Location = new System.Drawing.Point(4, 0);
            this.sortLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.sortLabel.Name = "sortLabel";
            this.sortLabel.Size = new System.Drawing.Size(120, 27);
            this.sortLabel.TabIndex = 1;
            this.sortLabel.Text = "Package source:";
            this.sortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // packageSourceComboBox
            // 
            this.packageSourceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.packageSourceComboBox.FormattingEnabled = true;
            this.packageSourceComboBox.Location = new System.Drawing.Point(132, 4);
            this.packageSourceComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.packageSourceComboBox.Name = "packageSourceComboBox";
            this.packageSourceComboBox.Size = new System.Drawing.Size(150, 24);
            this.packageSourceComboBox.TabIndex = 0;
            this.packageSourceComboBox.SelectedIndexChanged += new System.EventHandler(this.refreshButton_Click);
            // 
            // settingsButton
            // 
            this.settingsButton.FlatAppearance.BorderSize = 0;
            this.settingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsButton.Image = global::Bonsai.NuGet.Design.Properties.Resources.SettingsImage;
            this.settingsButton.Location = new System.Drawing.Point(289, 3);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(24, 23);
            this.settingsButton.TabIndex = 5;
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
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // settingsPanel
            // 
            this.settingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsPanel.Location = new System.Drawing.Point(4, 614);
            this.settingsPanel.Margin = new System.Windows.Forms.Padding(4);
            this.settingsPanel.Name = "settingsPanel";
            this.settingsPanel.Size = new System.Drawing.Size(727, 54);
            this.settingsPanel.TabIndex = 6;
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
            this.searchComboBox.TabIndex = 0;
            // 
            // packagePageSelector
            // 
            this.packagePageSelector.AutoSize = true;
            this.packagePageSelector.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packagePageSelector.Location = new System.Drawing.Point(0, 0);
            this.packagePageSelector.Margin = new System.Windows.Forms.Padding(5);
            this.packagePageSelector.Name = "packagePageSelector";
            this.packagePageSelector.Size = new System.Drawing.Size(719, 41);
            this.packagePageSelector.TabIndex = 0;
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
            this.packageView.Location = new System.Drawing.Point(4, 4);
            this.packageView.Margin = new System.Windows.Forms.Padding(4);
            this.packageView.Name = "packageView";
            this.packageView.OperationText = null;
            this.packageView.SelectedImageIndex = 0;
            this.packageView.ShowLines = false;
            this.packageView.ShowRootLines = false;
            this.packageView.Size = new System.Drawing.Size(711, 500);
            this.packageView.TabIndex = 1;
            this.packageView.OperationClick += new System.Windows.Forms.TreeViewEventHandler(this.packageView_OperationClick);
            // 
            // packageDetails
            // 
            this.packageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageDetails.Location = new System.Drawing.Point(5, 42);
            this.packageDetails.Margin = new System.Windows.Forms.Padding(5);
            this.packageDetails.Name = "packageDetails";
            this.packageDetails.Size = new System.Drawing.Size(319, 555);
            this.packageDetails.TabIndex = 1;
            // 
            // saveFolderDialog
            // 
            this.saveFolderDialog.FileName = "";
            // 
            // GalleryDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(1072, 672);
            this.Controls.Add(this.mainLayoutPanel);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1087, 709);
            this.Name = "GalleryDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bonsai - Gallery";
            this.mainLayoutPanel.ResumeLayout(false);
            this.packageViewLayoutPanel.ResumeLayout(false);
            this.filterLayoutPanel.ResumeLayout(false);
            this.pageSelectorPanel.ResumeLayout(false);
            this.pageSelectorPanel.PerformLayout();
            this.packageViewPanel.ResumeLayout(false);
            this.detailsLayoutPanel.ResumeLayout(false);
            this.searchLayoutPanel.ResumeLayout(false);
            this.closePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.ImageList packageIcons;
        private System.Windows.Forms.FlowLayoutPanel filterLayoutPanel;
        private System.Windows.Forms.ComboBox packageSourceComboBox;
        private System.Windows.Forms.Label sortLabel;
        private System.Windows.Forms.TableLayoutPanel packageViewLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel detailsLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel searchLayoutPanel;
        private Bonsai.NuGet.Design.CueBannerComboBox searchComboBox;
        private PackageDetails packageDetails;
        private System.Windows.Forms.Panel pageSelectorPanel;
        private PackagePageSelector packagePageSelector;
        private System.Windows.Forms.Panel closePanel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.TableLayoutPanel packageViewPanel;
        private PackageView packageView;
        private System.Windows.Forms.Panel settingsPanel;
        private SaveFolderDialog saveFolderDialog;
        private System.Windows.Forms.CheckBox prereleaseCheckBox;
        private System.Windows.Forms.Button settingsButton;
        private System.Windows.Forms.Button refreshButton;
    }
}
