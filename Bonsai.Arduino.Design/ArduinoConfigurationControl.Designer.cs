namespace Bonsai.Arduino.Design
{
    partial class ArduinoConfigurationControl
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
            this.portNameListbox = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.configurationManagerButton = new FlatButton();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // portNameListbox
            // 
            this.portNameListbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.portNameListbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.portNameListbox.FormattingEnabled = true;
            this.portNameListbox.Location = new System.Drawing.Point(0, 1);
            this.portNameListbox.Margin = new System.Windows.Forms.Padding(0, 1, 3, 0);
            this.portNameListbox.MinimumSize = new System.Drawing.Size(0, 17);
            this.portNameListbox.Name = "portNameListbox";
            this.portNameListbox.Size = new System.Drawing.Size(87, 20);
            this.portNameListbox.Sorted = true;
            this.portNameListbox.TabIndex = 0;
            this.portNameListbox.SelectedValueChanged += new System.EventHandler(this.portNameListbox_SelectedValueChanged);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.portNameListbox, 0, 0);
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

        private System.Windows.Forms.ListBox portNameListbox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private FlatButton configurationManagerButton;
    }
}
