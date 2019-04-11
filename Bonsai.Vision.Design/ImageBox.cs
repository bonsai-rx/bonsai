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
    public partial class ImageBox : VisualizerCanvas
    {
        bool disposed;
        IplImage image;
        bool allowUpdate;
        bool canvasInvalidated;
        IplImageTexture texture;

        public ImageBox()
        {
            ImageScale = 1.0;
        }

        internal double ImageScale { get; set; }

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

        protected virtual void SetImage(IplImage image)
        {
            MakeCurrent();
            texture.Update(image, ImageScale);
            Canvas.Invalidate();
            canvasInvalidated = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (DesignMode) return;
            allowUpdate = true;
            canvasInvalidated = false;
            texture = new IplImageTexture();
            base.OnLoad(e);
        }

        protected override void OnRenderFrame(EventArgs e)
        {
            if (image != null)
            {
                texture.Draw();
            }

            base.OnRenderFrame(e);
        }

        protected override void OnSwapBuffers(EventArgs e)
        {
            if (canvasInvalidated)
            {
                canvasInvalidated = false;
                allowUpdate = true;
            }
            base.OnSwapBuffers(e);
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
