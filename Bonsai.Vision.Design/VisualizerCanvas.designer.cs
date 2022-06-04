namespace Bonsai.Vision.Design
{
    partial class VisualizerCanvas
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.canvas = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // canvas
            // 
            this.canvas.BackColor = System.Drawing.Color.Black;
            this.canvas.Location = new System.Drawing.Point(0, 0);
            this.canvas.Name = "canvas";
            this.canvas.Size = new System.Drawing.Size(320, 240);
            this.canvas.TabIndex = 1;
            this.canvas.VSync = false;
            this.canvas.HandleCreated += new System.EventHandler(this.canvas_HandleCreated);
            this.canvas.Paint += new System.Windows.Forms.PaintEventHandler(this.canvas_Paint);
            this.canvas.Resize += new System.EventHandler(this.canvas_Resize);
            // 
            // VisualizerCanvas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.canvas);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "VisualizerCanvas";
            this.Size = new System.Drawing.Size(320, 240);
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl canvas;




    }
}
