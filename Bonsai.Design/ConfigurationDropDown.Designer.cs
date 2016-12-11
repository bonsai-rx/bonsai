namespace Bonsai.Design
{
    partial class ConfigurationDropDown
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
            this.configurationNameListbox = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.configurationManagerButton = new Bonsai.Design.ConfigurationDropDown.FlatButton();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // portNameListbox
            // 
            this.configurationNameListbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.configurationNameListbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurationNameListbox.FormattingEnabled = true;
            this.configurationNameListbox.IntegralHeight = false;
            this.configurationNameListbox.Location = new System.Drawing.Point(0, 1);
            this.configurationNameListbox.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.configurationNameListbox.MinimumSize = new System.Drawing.Size(0, 17);
            this.configurationNameListbox.Name = "portNameListbox";
            this.configurationNameListbox.Size = new System.Drawing.Size(87, 20);
            this.configurationNameListbox.TabIndex = 0;
            this.configurationNameListbox.SelectedValueChanged += new System.EventHandler(this.configurationNameListbox_SelectedValueChanged);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.configurationNameListbox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.configurationManagerButton, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(90, 56);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // configurationManagerButton
            // 
            this.configurationManagerButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurationManagerButton.FlatAppearance.BorderSize = 0;
            this.configurationManagerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.configurationManagerButton.Location = new System.Drawing.Point(0, 21);
            this.configurationManagerButton.Margin = new System.Windows.Forms.Padding(0);
            this.configurationManagerButton.Name = "configurationManagerButton";
            this.configurationManagerButton.Size = new System.Drawing.Size(90, 35);
            this.configurationManagerButton.TabIndex = 1;
            this.configurationManagerButton.Text = "Manage Ports";
            this.configurationManagerButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.configurationManagerButton.UseVisualStyleBackColor = true;
            this.configurationManagerButton.Click += new System.EventHandler(this.configurationManagerButton_Click);
            // 
            // ArduinoConfigurationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "ArduinoConfigurationControl";
            this.Size = new System.Drawing.Size(90, 56);
            this.tableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox configurationNameListbox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private FlatButton configurationManagerButton;
    }
}
