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
    public partial class IplImageControl : VisualizerCanvasControl
    {
        bool disposed;
        IplImage image;
        IplImageTexture texture;

        public IplImage Image
        {
            get { return image; }
            set
            {
                image = value;
                if (image != null)
                {
                    SetImage(image);
                }
            }
        }

        protected virtual void SetImage(IplImage image)
        {
            Canvas.MakeCurrent();
            texture.Update(image);
            Canvas.Invalidate();
        }

        protected override void OnLoad(EventArgs e)
        {
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
                    if (texture != null)
                    {
                        texture.Dispose();
                        texture = null;
                    }

                    disposed = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}
