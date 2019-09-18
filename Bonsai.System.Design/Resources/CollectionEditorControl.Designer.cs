namespace Bonsai.Resources.Design
{
    partial class CollectionEditorControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CollectionEditorControl));
            this.selectionListBox = new System.Windows.Forms.ListBox();
            this.removeButton = new System.Windows.Forms.Button();
            this.upButton = new System.Windows.Forms.Button();
            this.downButton = new System.Windows.Forms.Button();
            this.addButton = new Bonsai.Resources.Design.ContextMenuButton();
            this.SuspendLayout();
            // 
            // selectionListBox
            // 
            this.selectionListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.selectionListBox.FormattingEnabled = true;
            this.selectionListBox.IntegralHeight = false;
            this.selectionListBox.Location = new System.Drawing.Point(3, 4);
            this.selectionListBox.Name = "selectionListBox";
            this.selectionListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.selectionListBox.Size = new System.Drawing.Size(156, 160);
            this.selectionListBox.TabIndex = 0;
            this.selectionListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.selectionListBox_DrawItem);
            this.selectionListBox.SelectedIndexChanged += new System.EventHandler(this.selectionListBox_SelectedIndexChanged);
            this.selectionListBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.selectionListBox_DragDrop);
            this.selectionListBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.selectionListBox_DragEnter);
            this.selectionListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.selectionListBox_KeyDown);
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(84, 170);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(75, 23);
            this.removeButton.TabIndex = 3;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // upButton
            // 
            this.upButton.Image = ((System.Drawing.Image)(resources.GetObject("upButton.Image")));
            this.upButton.Location = new System.Drawing.Point(165, 4);
            this.upButton.Name = "upButton";
            this.upButton.Size = new System.Drawing.Size(24, 24);
            this.upButton.TabIndex = 5;
            this.upButton.UseVisualStyleBackColor = true;
            this.upButton.Click += new System.EventHandler(this.upButton_Click);
            // 
            // downButton
            // 
            this.downButton.Image = ((System.Drawing.Image)(resources.GetObject("downButton.Image")));
            this.downButton.Location = new System.Drawing.Point(165, 34);
            this.downButton.Name = "downButton";
            this.downButton.Size = new System.Drawing.Size(24, 24);
            this.downButton.TabIndex = 6;
            this.downButton.UseVisualStyleBackColor = true;
            this.downButton.Click += new System.EventHandler(this.downButton_Click);
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(3, 170);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(75, 23);
            this.addButton.TabIndex = 7;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // CollectionEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.downButton);
            this.Controls.Add(this.upButton);
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.selectionListBox);
            this.Name = "CollectionEditorControl";
            this.Size = new System.Drawing.Size(192, 196);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button upButton;
        private System.Windows.Forms.Button downButton;
        private System.Windows.Forms.ListBox selectionListBox;
        private System.Windows.Forms.Button removeButton;
        private ContextMenuButton addButton;
    }
}
