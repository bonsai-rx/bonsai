namespace Bonsai.NuGet
{
    partial class PackageSourceConfigurationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageSourceConfigurationDialog));
            this.moveDownButton = new System.Windows.Forms.Button();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.sourceTextBox = new System.Windows.Forms.TextBox();
            this.sourceLabel = new System.Windows.Forms.Label();
            this.sourceEditorButton = new System.Windows.Forms.Button();
            this.bottomLine = new System.Windows.Forms.Label();
            this.packageSourceListLabel = new System.Windows.Forms.Label();
            this.packageSourceListView = new System.Windows.Forms.ListView();
            this.nameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sourceHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkBoxesImageList = new System.Windows.Forms.ImageList(this.components);
            this.machineWideListLabel = new System.Windows.Forms.Label();
            this.machineWideListView = new System.Windows.Forms.ListView();
            this.machineWideNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.machineWideSourceHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.folderBrowserDialog = new Bonsai.Design.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // moveDownButton
            // 
            this.moveDownButton.Enabled = false;
            this.moveDownButton.Image = ((System.Drawing.Image)(resources.GetObject("moveDownButton.Image")));
            this.moveDownButton.Location = new System.Drawing.Point(447, 12);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(25, 25);
            this.moveDownButton.TabIndex = 4;
            this.moveDownButton.UseVisualStyleBackColor = true;
            this.moveDownButton.Click += new System.EventHandler(this.moveDownButton_Click);
            // 
            // moveUpButton
            // 
            this.moveUpButton.Enabled = false;
            this.moveUpButton.Image = ((System.Drawing.Image)(resources.GetObject("moveUpButton.Image")));
            this.moveUpButton.Location = new System.Drawing.Point(416, 12);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(25, 25);
            this.moveUpButton.TabIndex = 3;
            this.moveUpButton.UseVisualStyleBackColor = true;
            this.moveUpButton.Click += new System.EventHandler(this.moveUpButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Enabled = false;
            this.removeButton.Image = ((System.Drawing.Image)(resources.GetObject("removeButton.Image")));
            this.removeButton.Location = new System.Drawing.Point(385, 12);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(25, 25);
            this.removeButton.TabIndex = 2;
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // addButton
            // 
            this.addButton.Image = ((System.Drawing.Image)(resources.GetObject("addButton.Image")));
            this.addButton.Location = new System.Drawing.Point(354, 12);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(25, 25);
            this.addButton.TabIndex = 1;
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(397, 376);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 12;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(316, 376);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 11;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(12, 314);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(38, 13);
            this.nameLabel.TabIndex = 8;
            this.nameLabel.Text = "Name:";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(62, 311);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(296, 20);
            this.nameTextBox.TabIndex = 7;
            this.nameTextBox.Leave += new System.EventHandler(this.UpdatePackageSource);
            // 
            // sourceTextBox
            // 
            this.sourceTextBox.Location = new System.Drawing.Point(62, 337);
            this.sourceTextBox.Name = "sourceTextBox";
            this.sourceTextBox.Size = new System.Drawing.Size(296, 20);
            this.sourceTextBox.TabIndex = 8;
            this.sourceTextBox.Leave += new System.EventHandler(this.UpdatePackageSource);
            // 
            // sourceLabel
            // 
            this.sourceLabel.AutoSize = true;
            this.sourceLabel.Location = new System.Drawing.Point(12, 340);
            this.sourceLabel.Name = "sourceLabel";
            this.sourceLabel.Size = new System.Drawing.Size(44, 13);
            this.sourceLabel.TabIndex = 10;
            this.sourceLabel.Text = "Source:";
            // 
            // sourceEditorButton
            // 
            this.sourceEditorButton.AutoSize = true;
            this.sourceEditorButton.Location = new System.Drawing.Point(364, 334);
            this.sourceEditorButton.Name = "sourceEditorButton";
            this.sourceEditorButton.Size = new System.Drawing.Size(27, 23);
            this.sourceEditorButton.TabIndex = 9;
            this.sourceEditorButton.Text = "...";
            this.sourceEditorButton.UseVisualStyleBackColor = true;
            this.sourceEditorButton.Click += new System.EventHandler(this.sourceEditorButton_Click);
            // 
            // bottomLine
            // 
            this.bottomLine.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.bottomLine.Location = new System.Drawing.Point(12, 361);
            this.bottomLine.Name = "bottomLine";
            this.bottomLine.Size = new System.Drawing.Size(460, 2);
            this.bottomLine.TabIndex = 13;
            // 
            // packageSourceListLabel
            // 
            this.packageSourceListLabel.AutoSize = true;
            this.packageSourceListLabel.Location = new System.Drawing.Point(12, 17);
            this.packageSourceListLabel.Name = "packageSourceListLabel";
            this.packageSourceListLabel.Size = new System.Drawing.Size(138, 13);
            this.packageSourceListLabel.TabIndex = 14;
            this.packageSourceListLabel.Text = "Available package sources:";
            // 
            // packageSourceListView
            // 
            this.packageSourceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameHeader,
            this.sourceHeader});
            this.packageSourceListView.FullRowSelect = true;
            this.packageSourceListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.packageSourceListView.LargeImageList = this.checkBoxesImageList;
            this.packageSourceListView.Location = new System.Drawing.Point(12, 43);
            this.packageSourceListView.MultiSelect = false;
            this.packageSourceListView.Name = "packageSourceListView";
            this.packageSourceListView.ShowGroups = false;
            this.packageSourceListView.Size = new System.Drawing.Size(460, 139);
            this.packageSourceListView.TabIndex = 5;
            this.packageSourceListView.TileSize = new System.Drawing.Size(435, 30);
            this.packageSourceListView.UseCompatibleStateImageBehavior = false;
            this.packageSourceListView.View = System.Windows.Forms.View.Tile;
            this.packageSourceListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.packageSourceListView_ItemSelectionChanged);
            this.packageSourceListView.Enter += new System.EventHandler(this.packageSourceListView_Enter);
            this.packageSourceListView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.packageSourceListView_KeyPress);
            this.packageSourceListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.packageSourceListView_MouseClick);
            // 
            // nameHeader
            // 
            this.nameHeader.Width = 526;
            // 
            // checkBoxesImageList
            // 
            this.checkBoxesImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("checkBoxesImageList.ImageStream")));
            this.checkBoxesImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.checkBoxesImageList.Images.SetKeyName(0, "unchecked");
            this.checkBoxesImageList.Images.SetKeyName(1, "checked");
            // 
            // machineWideListLabel
            // 
            this.machineWideListLabel.AutoSize = true;
            this.machineWideListLabel.Location = new System.Drawing.Point(12, 195);
            this.machineWideListLabel.Name = "machineWideListLabel";
            this.machineWideListLabel.Size = new System.Drawing.Size(161, 13);
            this.machineWideListLabel.TabIndex = 17;
            this.machineWideListLabel.Text = "Machine-wide package sources:";
            // 
            // machineWideListView
            // 
            this.machineWideListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.machineWideNameHeader,
            this.machineWideSourceHeader});
            this.machineWideListView.FullRowSelect = true;
            this.machineWideListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.machineWideListView.LargeImageList = this.checkBoxesImageList;
            this.machineWideListView.Location = new System.Drawing.Point(12, 220);
            this.machineWideListView.MultiSelect = false;
            this.machineWideListView.Name = "machineWideListView";
            this.machineWideListView.ShowGroups = false;
            this.machineWideListView.Size = new System.Drawing.Size(460, 85);
            this.machineWideListView.TabIndex = 6;
            this.machineWideListView.TileSize = new System.Drawing.Size(435, 30);
            this.machineWideListView.UseCompatibleStateImageBehavior = false;
            this.machineWideListView.View = System.Windows.Forms.View.Tile;
            this.machineWideListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.packageSourceListView_ItemSelectionChanged);
            this.machineWideListView.Enter += new System.EventHandler(this.packageSourceListView_Enter);
            this.machineWideListView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.packageSourceListView_KeyPress);
            this.machineWideListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.packageSourceListView_MouseClick);
            // 
            // machineWideNameHeader
            // 
            this.machineWideNameHeader.Width = 526;
            // 
            // PackageSourceConfigurationDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 411);
            this.Controls.Add(this.machineWideListLabel);
            this.Controls.Add(this.machineWideListView);
            this.Controls.Add(this.packageSourceListLabel);
            this.Controls.Add(this.bottomLine);
            this.Controls.Add(this.sourceEditorButton);
            this.Controls.Add(this.sourceTextBox);
            this.Controls.Add(this.sourceLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.moveUpButton);
            this.Controls.Add(this.moveDownButton);
            this.Controls.Add(this.packageSourceListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PackageSourceConfigurationDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Options";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button moveDownButton;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button addButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.TextBox sourceTextBox;
        private System.Windows.Forms.Label sourceLabel;
        private System.Windows.Forms.Button sourceEditorButton;
        private System.Windows.Forms.Label bottomLine;
        private System.Windows.Forms.Label packageSourceListLabel;
        private Bonsai.Design.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ListView packageSourceListView;
        private System.Windows.Forms.ColumnHeader nameHeader;
        private System.Windows.Forms.ColumnHeader sourceHeader;
        private System.Windows.Forms.ImageList checkBoxesImageList;
        private System.Windows.Forms.Label machineWideListLabel;
        private System.Windows.Forms.ListView machineWideListView;
        private System.Windows.Forms.ColumnHeader machineWideNameHeader;
        private System.Windows.Forms.ColumnHeader machineWideSourceHeader;
    }
}