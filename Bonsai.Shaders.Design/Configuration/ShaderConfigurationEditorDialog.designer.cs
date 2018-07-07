namespace Bonsai.Shaders.Configuration.Design
{
    partial class ShaderConfigurationEditorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShaderConfigurationEditorDialog));
            this.saveButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.memberLabel = new System.Windows.Forms.Label();
            this.propertiesLabel = new System.Windows.Forms.Label();
            this.propertyGrid = new Bonsai.Design.PropertyGrid();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.windowButton = new System.Windows.Forms.RadioButton();
            this.meshButton = new System.Windows.Forms.RadioButton();
            this.textureButton = new System.Windows.Forms.RadioButton();
            this.shaderButton = new System.Windows.Forms.RadioButton();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.glslScriptEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.meshCollectionEditor = new Bonsai.Shaders.Configuration.Design.CollectionEditorControl();
            this.textureCollectionEditor = new Bonsai.Shaders.Configuration.Design.CollectionEditorControl();
            this.shaderCollectionEditor = new Bonsai.Shaders.Configuration.Design.ShaderCollectionEditorControl();
            this.flowLayoutPanel.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(272, 366);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 23);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(353, 366);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 1;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // memberLabel
            // 
            this.memberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.memberLabel.AutoSize = true;
            this.memberLabel.Location = new System.Drawing.Point(16, 34);
            this.memberLabel.Name = "memberLabel";
            this.memberLabel.Size = new System.Drawing.Size(53, 13);
            this.memberLabel.TabIndex = 5;
            this.memberLabel.Text = "Members:";
            // 
            // propertiesLabel
            // 
            this.propertiesLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertiesLabel.AutoSize = true;
            this.propertiesLabel.Location = new System.Drawing.Point(229, 34);
            this.propertiesLabel.Name = "propertiesLabel";
            this.propertiesLabel.Size = new System.Drawing.Size(57, 13);
            this.propertiesLabel.TabIndex = 2;
            this.propertiesLabel.Text = "Properties:";
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(232, 50);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(196, 310);
            this.propertyGrid.TabIndex = 9;
            this.propertyGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid_PropertyValueChanged);
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.flowLayoutPanel.Controls.Add(this.windowButton);
            this.flowLayoutPanel.Controls.Add(this.meshButton);
            this.flowLayoutPanel.Controls.Add(this.meshCollectionEditor);
            this.flowLayoutPanel.Controls.Add(this.textureButton);
            this.flowLayoutPanel.Controls.Add(this.textureCollectionEditor);
            this.flowLayoutPanel.Controls.Add(this.shaderButton);
            this.flowLayoutPanel.Controls.Add(this.shaderCollectionEditor);
            this.flowLayoutPanel.Location = new System.Drawing.Point(12, 50);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(200, 371);
            this.flowLayoutPanel.TabIndex = 4;
            // 
            // windowButton
            // 
            this.windowButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.windowButton.Location = new System.Drawing.Point(7, 7);
            this.windowButton.Margin = new System.Windows.Forms.Padding(7, 7, 7, 4);
            this.windowButton.Name = "windowButton";
            this.windowButton.Size = new System.Drawing.Size(185, 24);
            this.windowButton.TabIndex = 2;
            this.windowButton.Text = "Window";
            this.windowButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.windowButton.UseVisualStyleBackColor = true;
            this.windowButton.CheckedChanged += new System.EventHandler(this.windowButton_CheckedChanged);
            // 
            // meshButton
            // 
            this.meshButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.meshButton.Location = new System.Drawing.Point(7, 42);
            this.meshButton.Margin = new System.Windows.Forms.Padding(7, 7, 7, 4);
            this.meshButton.Name = "meshButton";
            this.meshButton.Size = new System.Drawing.Size(185, 24);
            this.meshButton.TabIndex = 3;
            this.meshButton.Text = "Meshes";
            this.meshButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.meshButton.UseVisualStyleBackColor = true;
            this.meshButton.CheckedChanged += new System.EventHandler(this.shaderButton_CheckedChanged);
            // 
            // textureButton
            // 
            this.textureButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.textureButton.Location = new System.Drawing.Point(7, 279);
            this.textureButton.Margin = new System.Windows.Forms.Padding(7, 7, 7, 4);
            this.textureButton.Name = "textureButton";
            this.textureButton.Size = new System.Drawing.Size(185, 24);
            this.textureButton.TabIndex = 4;
            this.textureButton.Text = "Textures";
            this.textureButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.textureButton.UseVisualStyleBackColor = true;
            this.textureButton.CheckedChanged += new System.EventHandler(this.shaderButton_CheckedChanged);
            // 
            // shaderButton
            // 
            this.shaderButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.shaderButton.Location = new System.Drawing.Point(7, 516);
            this.shaderButton.Margin = new System.Windows.Forms.Padding(7, 7, 7, 4);
            this.shaderButton.Name = "shaderButton";
            this.shaderButton.Size = new System.Drawing.Size(185, 24);
            this.shaderButton.TabIndex = 5;
            this.shaderButton.Text = "Shaders";
            this.shaderButton.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.shaderButton.UseVisualStyleBackColor = true;
            this.shaderButton.CheckedChanged += new System.EventHandler(this.shaderButton_CheckedChanged);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(440, 24);
            this.menuStrip.TabIndex = 10;
            this.menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.closeToolStripMenuItem.Text = "&Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("cutToolStripMenuItem.Image")));
            this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.cutToolStripMenuItem.Text = "Cu&t";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripMenuItem.Image")));
            this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.copyToolStripMenuItem.Text = "&Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripMenuItem.Image")));
            this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.pasteToolStripMenuItem.Text = "&Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.glslScriptEditorToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // glslScriptEditorToolStripMenuItem
            // 
            this.glslScriptEditorToolStripMenuItem.Name = "glslScriptEditorToolStripMenuItem";
            this.glslScriptEditorToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.glslScriptEditorToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.glslScriptEditorToolStripMenuItem.Text = "S&hader Editor";
            this.glslScriptEditorToolStripMenuItem.Click += new System.EventHandler(this.glslScriptEditorToolStripMenuItem_Click);
            // 
            // meshCollectionEditor
            // 
            this.meshCollectionEditor.AutoSize = true;
            this.meshCollectionEditor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.meshCollectionEditor.CollectionItemType = null;
            this.meshCollectionEditor.Location = new System.Drawing.Point(3, 73);
            this.meshCollectionEditor.Name = "meshCollectionEditor";
            this.meshCollectionEditor.NewItemTypes = null;
            this.meshCollectionEditor.SelectedItem = null;
            this.meshCollectionEditor.Size = new System.Drawing.Size(192, 196);
            this.meshCollectionEditor.TabIndex = 6;
            this.meshCollectionEditor.Visible = false;
            this.meshCollectionEditor.SelectedItemChanged += new System.EventHandler(this.collectionEditor_SelectedItemChanged);
            // 
            // textureCollectionEditor
            // 
            this.textureCollectionEditor.AutoSize = true;
            this.textureCollectionEditor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.textureCollectionEditor.CollectionItemType = null;
            this.textureCollectionEditor.Location = new System.Drawing.Point(3, 310);
            this.textureCollectionEditor.Name = "textureCollectionEditor";
            this.textureCollectionEditor.NewItemTypes = null;
            this.textureCollectionEditor.SelectedItem = null;
            this.textureCollectionEditor.Size = new System.Drawing.Size(192, 196);
            this.textureCollectionEditor.TabIndex = 7;
            this.textureCollectionEditor.Visible = false;
            this.textureCollectionEditor.SelectedItemChanged += new System.EventHandler(this.collectionEditor_SelectedItemChanged);
            // 
            // shaderCollectionEditor
            // 
            this.shaderCollectionEditor.AutoSize = true;
            this.shaderCollectionEditor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.shaderCollectionEditor.CollectionItemType = null;
            this.shaderCollectionEditor.Location = new System.Drawing.Point(3, 547);
            this.shaderCollectionEditor.Name = "shaderCollectionEditor";
            this.shaderCollectionEditor.NewItemTypes = null;
            this.shaderCollectionEditor.SelectedItem = null;
            this.shaderCollectionEditor.Size = new System.Drawing.Size(192, 196);
            this.shaderCollectionEditor.TabIndex = 8;
            this.shaderCollectionEditor.Visible = false;
            this.shaderCollectionEditor.SelectedItemChanged += new System.EventHandler(this.collectionEditor_SelectedItemChanged);
            // 
            // ShaderConfigurationEditorDialog
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(440, 401);
            this.Controls.Add(this.memberLabel);
            this.Controls.Add(this.flowLayoutPanel);
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.propertiesLabel);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.menuStrip);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(456, 415);
            this.Name = "ShaderConfigurationEditorDialog";
            this.ShowIcon = false;
            this.Text = "Shader Configuration";
            this.flowLayoutPanel.ResumeLayout(false);
            this.flowLayoutPanel.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Label propertiesLabel;
        private Bonsai.Design.PropertyGrid propertyGrid;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private ShaderCollectionEditorControl shaderCollectionEditor;
        private CollectionEditorControl textureCollectionEditor;
        private CollectionEditorControl meshCollectionEditor;
        private System.Windows.Forms.Label memberLabel;
        private System.Windows.Forms.RadioButton shaderButton;
        private System.Windows.Forms.RadioButton meshButton;
        private System.Windows.Forms.RadioButton textureButton;
        private System.Windows.Forms.RadioButton windowButton;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem glslScriptEditorToolStripMenuItem;
    }
}

