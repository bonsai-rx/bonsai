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
    public class FramebufferTexture : TextureConfiguration
    {
        int width;
        int height;
        int fbo;
        AttachmentTexture back;
        AttachmentTexture front;

        public FramebufferTexture()
        {
            Attachment = FramebufferAttachment.ColorAttachment0;
            Format = PixelInternalFormat.Rgba;
            ClearColor = Color.Transparent;
            back = new AttachmentTexture();
            front = new AttachmentTexture();
        }

        [Category("TextureSize")]
        public int? Width { get; set; }

        [Category("TextureSize")]
        public int? Height { get; set; }

        [Category("TextureParameter")]
        public TextureWrapMode WrapS
        {
            get { return back.WrapS; }
            set { back.WrapS = front.WrapS = value; }
        }

        [Category("TextureParameter")]
        public TextureWrapMode WrapT
        {
            get { return back.WrapT; }
            set { back.WrapT = front.WrapT = value; }
        }

        [Category("TextureParameter")]
        public TextureMinFilter MinFilter
        {
            get { return back.MinFilter; }
            set { back.MinFilter = front.MinFilter = value; }
        }

        [Category("TextureParameter")]
        public TextureMinFilter MagFilter
        {
            get { return back.MagFilter; }
            set { back.MagFilter = front.MagFilter = value; }
        }

        public FramebufferAttachment Attachment { get; set; }

        public TextureUnit? TextureSlot { get; set; }

        public PixelInternalFormat Format { get; set; }

        [XmlIgnore]
        public Color ClearColor { get; set; }

        [Browsable(false)]
        [XmlElement("ClearColor")]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        [Browsable(false)]
        public int Framebuffer
        {
            get { return fbo; }
        }

        public override int GetTexture()
        {
            return back.GetTexture();
        }

        void ClearTexture(int texture, int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                Format, width, height, 0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Load(Shader shader)
        {
            back.Load(shader);
            var texture = back.GetTexture();
            width = Width.GetValueOrDefault(shader.Window.Width);
            height = Height.GetValueOrDefault(shader.Window.Height);
            ClearTexture(texture, width, height);
            if (TextureSlot.HasValue)
            {
                front.Load(shader);
                ClearTexture(front.GetTexture(), width, height);
                shader.SetTextureSlot(Name, TextureSlot.Value);
            }

            GL.GenFramebuffers(1, out fbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachment, TextureTarget.Texture2D, texture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public override void Bind(Shader shader)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            if (TextureSlot.HasValue)
            {
                var temp = back;
                back = front;
                front = temp;
                GL.ActiveTexture(TextureSlot.Value);
                GL.BindTexture(TextureTarget.Texture2D, front.GetTexture());
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachment, TextureTarget.Texture2D, back.GetTexture(), 0);
            }

            GL.ClearColor(ClearColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shader.Window.UpdateViewport(width, height);
        }

        public override void Unbind(Shader shader)
        {
            if (TextureSlot.HasValue)
            {
                GL.ActiveTexture(TextureSlot.Value);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            shader.Window.UpdateViewport();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public override void Unload(Shader shader)
        {
            width = 0;
            height = 0;
            GL.DeleteFramebuffers(1, ref fbo);
            back.Unload(shader);
            if (TextureSlot.HasValue) front.Unload(shader);
        }

        #region AttachmentTexture Class

        class AttachmentTexture : TextureBase
        {
            public override void Bind(Shader shader)
            {
            }

            public override void Unbind(Shader shader)
            {
            }
        }

        #endregion
    }
}
