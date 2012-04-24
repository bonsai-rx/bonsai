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
    public partial class IplImageControl : UserControl
    {
        bool loaded;
        int texture;
        IplImage image;

        public IplImageControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
        }

        public GLControl PictureBox
        {
            get { return glControl; }
        }

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
            if (image == null) throw new ArgumentNullException("image");
            if (image.Depth != 8) throw new ArgumentException("Non 8-bit depth images are not supported by the control.", "image");

            OpenTK.Graphics.OpenGL.PixelFormat pixelFormat;
            switch (image.NumChannels)
            {
                case 1: pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Luminance; break;
                case 3: pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgr; break;
                case 4: pixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra; break;
                default: throw new ArgumentException("Image has an unsupported number of channels.", "image");
            }

            glControl.MakeCurrent();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, pixelFormat, PixelType.UnsignedByte, image.ImageData);
            glControl.Invalidate();
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            glControl.MakeCurrent();
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 1.0);

            loaded = true;
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            if (!loaded) return;

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            if (!loaded) return;

            glControl.MakeCurrent();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            RenderImage();
        }

        void RenderImage()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            GL.Begin(BeginMode.Quads);

            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1f, -1f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1f, -1f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1f, 1f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1f, 1f);

            GL.End();

            glControl.SwapBuffers();
        }
    }
}
