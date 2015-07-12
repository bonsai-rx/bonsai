using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    public class FramebufferTexture : TextureBase
    {
        int width;
        int height;
        int fbo;

        public FramebufferTexture()
        {
            Attachment = FramebufferAttachment.ColorAttachment0;
            ClearColor = Color.Transparent;
        }

        [Category("TextureSize")]
        public int? Width { get; set; }

        [Category("TextureSize")]
        public int? Height { get; set; }

        public FramebufferAttachment Attachment { get; set; }

        [XmlIgnore]
        public Color ClearColor { get; set; }

        [Browsable(false)]
        [XmlElement("ClearColor")]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        public override void Load(Shader shader)
        {
            base.Load(shader);
            var texture = GetTexture();
            width = Width.GetValueOrDefault(shader.Window.Width);
            height = Height.GetValueOrDefault(shader.Window.Height);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba,
                width, height, 0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachment, TextureTarget.Texture2D, texture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public override void Bind(Shader shader)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.ClearColor(ClearColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, width, height);
        }

        public override void Unbind(Shader shader)
        {
            GL.Viewport(shader.Window.ClientRectangle);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public override void Unload(Shader shader)
        {
            width = 0;
            height = 0;
            GL.DeleteFramebuffers(1, ref fbo);
            base.Unload(shader);
        }
    }
}
