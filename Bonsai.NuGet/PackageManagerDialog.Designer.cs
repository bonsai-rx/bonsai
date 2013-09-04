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
            this.packageView = new Bonsai.NuGet.PackageView();
            this.packageIcons = new System.Windows.Forms.ImageList(this.components);
            this.filterLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.releaseFilterComboBox = new System.Windows.Forms.ComboBox();
            this.sortLabel = new System.Windows.Forms.Label();
            this.sortComboBox = new System.Windows.Forms.ComboBox();
            this.pageSelectorPanel = new System.Windows.Forms.Panel();
            this.packagePageSelector = new Bonsai.NuGet.PackagePageSelector();
            this.detailsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.searchLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.searchComboBox = new System.Windows.Forms.ComboBox();
            this.packageDetails = new Bonsai.NuGet.PackageDetails();
            this.repositoriesView = new System.Windows.Forms.TreeView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.closeButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.settingsButton = new System.Windows.Forms.Button();
            this.mainLayoutPanel.SuspendLayout();
            this.packageViewLayoutPanel.SuspendLayout();
            this.filterLayoutPanel.SuspendLayout();
            this.pageSelectorPanel.SuspendLayout();
            this.detailsLayoutPanel.SuspendLayout();
            this.searchLayoutPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 3;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 225F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 253F));
            this.mainLayoutPanel.Controls.Add(this.packageViewLayoutPanel, 1, 0);
            this.mainLayoutPanel.Controls.Add(this.detailsLayoutPanel, 2, 0);
            this.mainLayoutPanel.Controls.Add(this.repositoriesView, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.panel1, 2, 1);
            this.mainLayoutPanel.Controls.Add(this.panel2, 0, 1);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 2;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainLayoutPanel.Size = new System.Drawing.Size(804, 546);
            this.mainLayoutPanel.TabIndex = 0;
            // 
            // packageViewLayoutPanel
            // 
            this.packageViewLayoutPanel.ColumnCount = 1;
            this.packageViewLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewLayoutPanel.Controls.Add(this.packageView, 0, 1);
            this.packageViewLayoutPanel.Controls.Add(this.filterLayoutPanel, 0, 0);
            this.packageViewLayoutPanel.Controls.Add(this.pageSelectorPanel, 0, 2);
            this.packageViewLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageViewLayoutPanel.Location = new System.Drawing.Point(228, 3);
            this.packageViewLayoutPanel.Name = "packageViewLayoutPanel";
            this.packageViewLayoutPanel.RowCount = 3;
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.packageViewLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.packageViewLayoutPanel.Size = new System.Drawing.Size(320, 490);
            this.packageViewLayoutPanel.TabIndex = 2;
            // 
            // packageView
            // 
            this.packageView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this.packageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.packageView.FullRowSelect = true;
            this.packageView.ImageIndex = 0;
            this.packageView.ImageList = this.packageIcons;
            this.packageView.ItemHeight = 64;
            this.packageView.Location = new System.Drawing.Point(3, 33);
            this.packageView.Name = "packageView";
            this.packageView.OperationText = null;
            this.packageView.SelectedImageIndex = 0;
            this.packageView.ShowRootLines = false;
            this.packageView.Size = new System.Drawing.Size(314, 414);
            this.packageView.TabIndex = 0;
            this.packageView.OperationClick += new System.Windows.Forms.TreeViewEventHandler(this.packageView_OperationClick);
            this.packageView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.packageView_AfterSelect);
            // 
            // packageIcons
            // 
            this.packageIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.packageIcons.ImageSize = new System.Drawing.Size(32, 32);
            this.packageIcons.TransparentColor = System.Drawing.Color.Violet;
            // 
            // filterLayoutPanel
            // 
            this.filterLayoutPanel.Controls.Add(this.releaseFilterComboBox);
            this.filterLayoutPanel.Controls.Add(this.sortLabel);
            this.filterLayoutPanel.Controls.Add(this.sortComboBox);
            this.filterLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.filterLayoutPanel.Name = "filterLayoutPanel";
            this.filterLayoutPanel.Size = new System.Drawing.Size(314, 24);
            this.filterLayoutPanel.TabIndex = 1;
            // 
            // releaseFilterComboBox
            // 
            this.releaseFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.releaseFilterComboBox.FormattingEnabled = true;
            this.releaseFilterComboBox.Items.AddRange(new object[] {
            "Stable Only",
            "Include Prerelease"});
            this.releaseFilterComboBox.Location = new System.Drawing.Point(3, 3);
            this.releaseFilterComboBox.Name = "releaseFilterComboBox";
            this.releaseFilterComboBox.Size = new System.Drawing.Size(121, 21);
            this.releaseFilterComboBox.TabIndex = 0;
            this.releaseFilterComboBox.SelectedIndexChanged += new System.EventHandler(this.filterComboBox_SelectedIndexChanged);
            // 
            // sortLabel
            // 
            this.sortLabel.Location = new System.Drawing.Point(130, 0);
            this.sortLabel.Name = "sortLabel";
            this.sortLabel.Size = new System.Drawing.Size(43, 22);
            this.sortLabel.TabIndex = 1;
            this.sortLabel.Text = "Sort by:";
            this.sortLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // sortComboBox
            // 
            this.sortComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortComboBox.FormattingEnabled = true;
            this.sortComboBox.Location = new System.Drawing.Point(179, 3);
            this.sortComboBox.Name = "sortComboBox";
            this.sortComboBox.Size = new System.Drawing.Size(121, 21);
            this.sortComboBox.TabIndex = 2;
            this.sortComboBox.SelectedIndexChanged += new System.EventHandler(this.filterComboBox_SelectedIndexChanged);
            // 
            // pageSelectorPanel
            // 
            this.pageSelectorPanel.Controls.Add(this.packagePageSelector);
            this.pageSelectorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pageSelectorPanel.Location = new System.Drawing.Point(3, 453);
            this.pageSelectorPanel.Name = "pageSelectorPanel";
            this.pageSelectorPanel.Size = new System.Drawing.Size(314, 34);
            this.pageSelectorPanel.TabIndex = 2;
            // 
            // packagePageSelector
            // 
            this.packagePageSelector.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.packagePageSelector.Location = new System.Drawing.Point(69, 0);
            this.packagePageSelector.Name = "packagePageSelector";
            this.packagePageSelector.PageCount = 0;
            this.packagePageSelector.SelectedIndex = -1;
            this.packagePageSelector.Size = new System.Drawing.Size(206, 32);
            this.packagePageSelector.TabIndex = 0;
            this.packagePageSelector.SelectedIndexChanged += new System.EventHandler(this.packagePageSelector_SelectedIndexChanged);
            // 
            // detailsLayoutPanel
            // 
            this.detailsLayoutPanel.ColumnCount = 1;
            this.detailsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailsLayoutPanel.Controls.Add(this.searchLayoutPanel, 0, 0);
            this.detailsLayoutPanel.Controls.Add(this.packageDetails, 0, 1);
            this.detailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailsLayoutPanel.Location = new System.Drawing.Point(554, 3);
            this.detailsLayoutPanel.Name = "detailsLayoutPanel";
            this.detailsLayoutPanel.RowCount = 2;
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.detailsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.detailsLayoutPanel.Size = new System.Drawing.Size(247, 490);
            this.detailsLayoutPanel.TabIndex = 3;
            // 
            // searchLayoutPanel
            // 
            this.searchLayoutPanel.Controls.Add(this.searchComboBox);
            this.searchLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.searchLayoutPanel.Name = "searchLayoutPanel";
            this.searchLayoutPanel.Size = new System.Drawing.Size(241, 24);
            this.searchLayoutPanel.TabIndex = 0;
            // 
            // searchComboBox
            // 
            this.searchComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.searchComboBox.FormattingEnabled = true;
            this.searchComboBox.Location = new System.Drawing.Point(3, 3);
            this.searchComboBox.Name = "searchComboBox";
            this.searchComboBox.Size = new System.Drawing.Size(147, 21);
            this.searchComboBox.TabIndex = 0;
            // 
            // packageDetails
            // 
            this.packageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.packageDetails.Location = new System.Drawing.Point(3, 33);
            this.packageDetails.Name = "packageDetails";
            this.packageDetails.Size = new System.Drawing.Size(241, 454);
            this.packageDetails.TabIndex = 1;
            // 
            // repositoriesView
            // 
            this.repositoriesView.BackColor = System.Drawing.SystemColors.Control;
            this.repositoriesView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.repositoriesView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.repositoriesView.FullRowSelect = true;
            this.repositoriesView.ItemHeight = 25;
            this.repositoriesView.Location = new System.Drawing.Point(3, 3);
            this.repositoriesView.Name = "repositoriesView";
            this.repositoriesView.ShowLines = false;
            this.repositoriesView.Size = new System.Drawing.Size(219, 490);
            this.repositoriesView.TabIndex = 4;
            this.repositoriesView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.repositoriesView_BeforeSelect);
            this.repositoriesView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.repositoriesView_AfterSelect);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.closeButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(554, 499);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(247, 44);
            this.panel1.TabIndex = 5;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(163, 12);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.settingsButton);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 499);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(219, 44);
            this.panel2.TabIndex = 6;
            // 
            // settingsButton
            // 
            this.settingsButton.Location = new System.Drawing.Point(9, 12);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(75, 23);
            this.settingsButton.TabIndex = 0;
            this.settingsButton.Text = "Settings";
            this.settingsButton.UseVisualStyleBackColor = true;
            // 
            // PackageManagerDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 546);
            this.Controls.Add(this.mainLayoutPanel);
            this.MinimumSize = new System.Drawing.Size(820, 585);
            this.Name = "PackageManagerDialog";
            this.ShowIcon = false;
            this.Text = "PackageManagerDialog";
            this.mainLayoutPanel.ResumeLayout(false);
            this.packageViewLayoutPanel.ResumeLayout(false);
            this.filterLayoutPanel.ResumeLayout(false);
            this.pageSelectorPanel.ResumeLayout(false);
            this.detailsLayoutPanel.ResumeLayout(false);
            this.searchLayoutPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.ImageList packageIcons;
        private PackageView packageView;
        private System.Windows.Forms.FlowLayoutPanel filterLayoutPanel;
        private System.Windows.Forms.ComboBox releaseFilterComboBox;
        private System.Windows.Forms.Label sortLabel;
        private System.Windows.Forms.ComboBox sortComboBox;
        private System.Windows.Forms.TableLayoutPanel packageViewLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel detailsLayoutPanel;
        private System.Windows.Forms.TreeView repositoriesView;
        private System.Windows.Forms.FlowLayoutPanel searchLayoutPanel;
        private System.Windows.Forms.ComboBox searchComboBox;
        private PackageDetails packageDetails;
        private System.Windows.Forms.Panel pageSelectorPanel;
        private PackagePageSelector packagePageSelector;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button settingsButton;
    }
}