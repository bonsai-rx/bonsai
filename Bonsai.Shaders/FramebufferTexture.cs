using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class FramebufferTexture : TextureBase
    {
        public FramebufferTexture()
        {
            Attachment = FramebufferAttachment.ColorAttachment0;
        }

        public FramebufferAttachment Attachment { get; set; }

        public override void Load(Shader shader)
        {
            base.Load(shader);
            var texture = GetTexture();
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                PixelInternalFormat.Rgba,
                shader.Window.Width,
                shader.Window.Height,
                0, PixelFormat.Rgba,
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
        }

        public override void Unbind(Shader shader)
        {
        }
    }
}
