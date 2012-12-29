using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCV.Net;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Bonsai.Vision.Design
{
    public partial class VisualizerCanvasControl : UserControl
    {
        bool loaded;
        bool disposed;

        public VisualizerCanvasControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
        }

        public event EventHandler RenderFrame;

        public GLControl Canvas
        {
            get { return canvas; }
        }

        private void canvas_Load(object sender, EventArgs e)
        {
            canvas.MakeCurrent();
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 1.0);

            loaded = true;
        }

        private void canvas_Resize(object sender, EventArgs e)
        {
            if (!loaded) return;

            canvas.MakeCurrent();
            GL.Viewport(0, 0, canvas.Width, canvas.Height);
            canvas.Invalidate();
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded) return;

            canvas.MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            OnRenderFrame(e);
            canvas.SwapBuffers();
        }

        protected virtual void OnRenderFrame(EventArgs e)
        {
            var handler = RenderFrame;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (components != null) components.Dispose();
                    disposed = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}
