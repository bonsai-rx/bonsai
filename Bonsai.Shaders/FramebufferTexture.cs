using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class FramebufferTexture : TextureBase
    {
        int width;
        int height;

        public FramebufferTexture()
        {
            Attachment = FramebufferAttachment.ColorAttachment0;
        }

        [Category("TextureSize")]
        public int? Width { get; set; }

        [Category("TextureSize")]
        public int? Height { get; set; }

        public FramebufferAttachment Attachment { get; set; }

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

            shader.EnsureFrameBuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shader.Framebuffer);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachment, TextureTarget.Texture2D, texture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public override void Bind(Shader shader)
        {
            GL.Viewport(0, 0, width, height);
        }

        public override void Unbind(Shader shader)
        {
            GL.Viewport(shader.Window.ClientRectangle);
        }

        public override void Unload(Shader shader)
        {
            width = 0;
            height = 0;
            base.Unload(shader);
        }
    }
}
