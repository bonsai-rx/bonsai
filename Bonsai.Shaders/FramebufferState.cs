using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class FramebufferState : IDisposable
    {
        readonly List<FramebufferAttachment> framebufferAttachments;
        ShaderWindow framebufferWindow;
        int framebufferWidth;
        int framebufferHeight;
        int fbo;

        public FramebufferState(ShaderWindow window, FramebufferConfiguration framebufferConfiguration)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            if (framebufferConfiguration == null)
            {
                throw new ArgumentNullException("framebufferConfiguration");
            }

            framebufferWidth = 0;
            framebufferHeight = 0;
            framebufferWindow = window;
            framebufferAttachments = framebufferConfiguration.FramebufferAttachments
                                                             .Select(configuration => new FramebufferAttachment(window, configuration))
                                                             .ToList();
            if (framebufferAttachments.Count > 0)
            {
                foreach (var attachment in framebufferAttachments)
                {
                    var width = attachment.Width;
                    var height = attachment.Height;
                    if (framebufferWidth == 0 || width < framebufferWidth)
                    {
                        framebufferWidth = width;
                    }

                    if (framebufferHeight == 0 || height < framebufferHeight)
                    {
                        framebufferHeight = height;
                    }
                }

                GL.GenFramebuffers(1, out fbo);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                foreach (var attachment in framebufferAttachments)
                {
                    attachment.Attach();
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }

        public ShaderWindow Window
        {
            get { return framebufferWindow; }
        }

        public void Bind()
        {
            if (fbo > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                foreach (var attachment in framebufferAttachments)
                {
                    attachment.Clear();
                }

                framebufferWindow.UpdateViewport(framebufferWidth, framebufferHeight);
                framebufferWindow.UpdateScissor(framebufferWidth, framebufferHeight);
            }
        }

        public void Unbind()
        {
            if (fbo > 0)
            {
                framebufferWindow.UpdateViewport();
                framebufferWindow.UpdateScissor();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }

        class FramebufferAttachment
        {
            Texture texture;
            readonly string textureName;
            readonly int textureWidth;
            readonly int textureHeight;
            readonly OpenTK.Graphics.OpenGL4.FramebufferAttachment attachment;
            readonly PixelInternalFormat internalFormat;
            readonly PixelFormat format;
            readonly PixelType type;
            readonly Color clearColor;
            readonly ClearBufferMask clearMask;

            public FramebufferAttachment(ShaderWindow window, FramebufferAttachmentConfiguration attachmentConfiguration)
            {
                textureName = attachmentConfiguration.TextureName;
                textureWidth = attachmentConfiguration.Width.GetValueOrDefault(window.Width);
                textureHeight = attachmentConfiguration.Height.GetValueOrDefault(window.Height);
                attachment = attachmentConfiguration.Attachment;
                internalFormat = attachmentConfiguration.InternalFormat;
                format = attachmentConfiguration.Format;
                type = attachmentConfiguration.Type;
                clearColor = attachmentConfiguration.ClearColor;
                clearMask = attachmentConfiguration.ClearMask;

                texture = window.ResourceManager.Load<Texture>(attachmentConfiguration.TextureName);
                ClearTexture(texture.Id, textureWidth, textureHeight);
            }

            public int Width
            {
                get { return textureWidth; }
            }

            public int Height
            {
                get { return textureHeight; }
            }

            void ClearTexture(int texture, int width, int height)
            {
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexImage2D(
                    TextureTarget.Texture2D, 0,
                    internalFormat, width, height, 0,
                    format,
                    type,
                    IntPtr.Zero);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            public void Attach()
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2D, texture.Id, 0);
            }

            public void Clear()
            {
                if (clearMask != ClearBufferMask.None)
                {
                    GL.ClearColor(clearColor);
                    GL.Clear(clearMask);
                }
            }
        }

        public void Dispose()
        {
            if (fbo > 0)
            {
                framebufferWidth = 0;
                framebufferHeight = 0;
                GL.DeleteFramebuffers(1, ref fbo);
            }
        }
    }
}
