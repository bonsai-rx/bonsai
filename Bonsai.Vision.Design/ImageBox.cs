using System;
using OpenCV.Net;

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Represents a graphics accelerated control for displaying an image.
    /// </summary>
    public partial class ImageBox : VisualizerCanvas
    {
        bool disposed;
        IplImage image;
        bool allowUpdate;
        bool canvasInvalidated;
        IplImageTexture texture;

        /// <summary>
        /// Gets or sets the brightness scale factor applied when rendering the image.
        /// </summary>
        public double ImageScale { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the image to display.
        /// </summary>
        public IplImage Image
        {
            get { return image; }
            set
            {
                image = value;
                if (image != null)
                {
                    if (InvokeRequired)
                    {
                        if (allowUpdate)
                        {
                            Action<IplImage> setImage = SetImage;
                            BeginInvoke(setImage, image);
                            allowUpdate = false;
                        }
                    }
                    else SetImage(image);
                }
            }
        }

        /// <summary>
        /// Updates the image to display in the control.
        /// </summary>
        /// <param name="image">The image to display.</param>
        protected virtual void SetImage(IplImage image)
        {
            MakeCurrent();
            texture.Update(image, ImageScale);
            Canvas.Invalidate();
            canvasInvalidated = true;
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode) return;
            allowUpdate = true;
            canvasInvalidated = false;
            texture = new IplImageTexture();
            base.OnLoad(e);
        }

        /// <inheritdoc/>
        protected override void OnRenderFrame(EventArgs e)
        {
            if (image != null)
            {
                texture.Draw();
            }

            base.OnRenderFrame(e);
        }

        /// <inheritdoc/>
        protected override void OnSwapBuffers(EventArgs e)
        {
            if (canvasInvalidated)
            {
                canvasInvalidated = false;
                allowUpdate = true;
            }
            base.OnSwapBuffers(e);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    MakeCurrent();
                    if (texture != null)
                    {
                        texture.Dispose();
                        texture = null;
                    }

                    allowUpdate = false;
                    disposed = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}
