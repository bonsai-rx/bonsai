using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using OpenTK.Graphics;

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a control with a graphics context and a simple render
    /// loop for scheduling accelerated rendering operations.
    /// </summary>
    public partial class VisualizerCanvas : UserControl
    {
        bool loaded;
        bool disposed;
        bool resizing;
        bool resizeWidth;
        static readonly object syncRoot = string.Intern("A1105A50-BBB0-4EC6-B8B2-B5EF38A9CC3E");

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerCanvas"/> class.
        /// </summary>
        public VisualizerCanvas()
        {
            GraphicsContext.ShareContexts = false;
            InitializeComponent();
        }

        /// <summary>
        /// Occurs when it is time to render a frame.
        /// </summary>
        public event EventHandler RenderFrame;

        /// <summary>
        /// Occurs immediately after the front and back buffers are swapped,
        /// and the rendered scene is presented to the screen.
        /// </summary>
        public event EventHandler SwapBuffers;

        /// <summary>
        /// Gets the control containing the graphics context on which to call
        /// render operations.
        /// </summary>
        public GLControl Canvas
        {
            get { return canvas; }
        }

        private void canvas_HandleCreated(object sender, EventArgs e)
        {
            if (DesignMode) return;
            MakeCurrent();
            GL.ClearColor(Color.Black);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 1.0);

            loaded = true;
            canvas.Size = Size;
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
                if (resizing)
                {
                    resizing = false;
                    resizeWidth = !resizeWidth;
                    if (canvas.Size != Size)
                    {
                        OnResize(EventArgs.Empty);
                    }
                }
            }
            OnSwapBuffers(e);
        }

        /// <inheritdoc/>
        protected override void OnResize(EventArgs e)
        {
            var partialResize = canvas.Width == Width || canvas.Height == Height;
            if (!loaded || partialResize) canvas.Size = Size;
            else
            {
                if (resizeWidth) canvas.Size = new Size(Width, canvas.Height);
                else canvas.Size = new Size(canvas.Width, Height);
                resizing = true;
            }
            base.OnResize(e);
        }

        /// <summary>
        /// Raises the <see cref="RenderFrame"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnRenderFrame(EventArgs e)
        {
            RenderFrame?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="SwapBuffers"/> event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="EventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnSwapBuffers(EventArgs e)
        {
            SwapBuffers?.Invoke(this, e);
        }

        /// <summary>
        /// Makes the canvas context current in the calling thread.
        /// </summary>
        public void MakeCurrent()
        {
            if (GraphicsContext.CurrentContext != canvas.Context)
            {
                canvas.MakeCurrent();
            }
        }

        /// <inheritdoc/>
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
