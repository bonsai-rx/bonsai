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
using OpenTK.Graphics;

namespace Bonsai.Vision.Design
{
    public partial class VisualizerCanvas : UserControl
    {
        bool loaded;
        bool disposed;
        static readonly object syncRoot = string.Intern("A1105A50-BBB0-4EC6-B8B2-B5EF38A9CC3E");

        public VisualizerCanvas()
        {
            GraphicsContext.ShareContexts = false;
            InitializeComponent();
        }

        public event EventHandler RenderFrame;

        public event EventHandler SwapBuffers;

        public GLControl Canvas
        {
            get { return canvas; }
        }

        private void canvas_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            MakeCurrent();
            GL.ClearColor(Color.Black);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 1.0);

            loaded = true;
        }

        private void canvas_Resize(object sender, EventArgs e)
        {
            if (!loaded) return;

            MakeCurrent();
            GL.Viewport(0, 0, canvas.Width, canvas.Height);
            canvas.Invalidate();
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded) return;

            MakeCurrent();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            OnRenderFrame(e);
            lock (syncRoot)
            {
                canvas.SwapBuffers();
            }
            OnSwapBuffers(e);
        }

        protected virtual void OnRenderFrame(EventArgs e)
        {
            var handler = RenderFrame;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSwapBuffers(EventArgs e)
        {
            var handler = SwapBuffers;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void MakeCurrent()
        {
            if (GraphicsContext.CurrentContext != canvas.Context)
            {
                canvas.MakeCurrent();
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
                    MakeCurrent();
                    if (components != null) components.Dispose();
                    disposed = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}
