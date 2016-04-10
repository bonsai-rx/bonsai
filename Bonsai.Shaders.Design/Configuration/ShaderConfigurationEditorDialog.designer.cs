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
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.memberLabel = new System.Windows.Forms.Label();
            this.propertiesLabel = new System.Windows.Forms.Label();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.windowButton = new System.Windows.Forms.RadioButton();
            this.meshButton = new System.Windows.Forms.RadioButton();
            this.textureButton = new System.Windows.Forms.RadioButton();
            this.shaderButton = new System.Windows.Forms.RadioButton();
            this.meshCollectionEditor = new Bonsai.Shaders.Configuration.Design.CollectionEditorControl();
            this.textureCollectionEditor = new Bonsai.Shaders.Configuration.Design.CollectionEditorControl();
            this.shaderCollectionEditor = new Bonsai.Shaders.Configuration.Design.CollectionEditorControl();
            this.flowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(272, 341);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(353, 341);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // memberLabel
            // 
            this.memberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.memberLabel.AutoSize = true;
            this.memberLabel.Location = new System.Drawing.Point(16, 9);
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
            this.propertiesLabel.Location = new System.Drawing.Point(229, 9);
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
            this.propertyGrid.Location = new System.Drawing.Point(232, 25);
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
            this.flowLayoutPanel.Location = new System.Drawing.Point(12, 25);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(200, 346);
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
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(440, 376);
            this.Controls.Add(this.memberLabel);
            this.Controls.Add(this.flowLayoutPanel);
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.propertiesLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(456, 415);
            this.Name = "ShaderConfigurationEditorDialog";
            this.ShowIcon = false;
            this.Text = "Shader Configuration Editor";
            this.flowLayoutPanel.ResumeLayout(false);
            this.flowLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label propertiesLabel;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private CollectionEditorControl shaderCollectionEditor;
        private CollectionEditorControl textureCollectionEditor;
        private CollectionEditorControl meshCollectionEditor;
        private System.Windows.Forms.Label memberLabel;
        private System.Windows.Forms.RadioButton shaderButton;
        private System.Windows.Forms.RadioButton meshButton;
        private System.Windows.Forms.RadioButton textureButton;
        private System.Windows.Forms.RadioButton windowButton;
    }
}

