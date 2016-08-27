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
    class FramebufferState
    {
        readonly List<FramebufferAttachment> framebufferAttachments;
        int framebufferWidth;
        int framebufferHeight;
        int fbo;

        public FramebufferState(FramebufferConfiguration framebufferConfiguration)
        {
            if (framebufferConfiguration == null)
            {
                throw new ArgumentNullException("framebufferConfiguration");
            }

            framebufferAttachments = framebufferConfiguration.FramebufferAttachments
                                                             .Select(configuration => new FramebufferAttachment(configuration))
                                                             .ToList();
        }

        public void Load(ShaderWindow window)
        {
            framebufferWidth = 0;
            framebufferHeight = 0;
            if (framebufferAttachments.Count > 0)
            {
                foreach (var attachment in framebufferAttachments)
                {
                    int width, height;
                    attachment.Load(window, out width, out height);
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

        public void Bind(ShaderWindow window)
        {
            if (fbo > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                foreach (var attachment in framebufferAttachments)
                {
                    attachment.Clear();
                }

                window.UpdateViewport(framebufferWidth, framebufferHeight);
            }
        }

        public void Unbind(ShaderWindow window)
        {
            if (fbo > 0)
            {
                window.UpdateViewport();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }

        public void Unload(ShaderWindow window)
        {
            if (fbo > 0)
            {
                foreach (var attachment in framebufferAttachments)
                {
                    attachment.Unload(window);
                }

                framebufferWidth = 0;
                framebufferHeight = 0;
                GL.DeleteFramebuffers(1, ref fbo);
            }
        }

        class FramebufferAttachment
        {
            Texture texture;
            readonly string textureName;
            readonly int? textureWidth;
            readonly int? textureHeight;
            readonly OpenTK.Graphics.OpenGL4.FramebufferAttachment attachment;
            readonly PixelInternalFormat internalFormat;
            readonly PixelFormat format;
            readonly PixelType type;
            readonly Color? clearColor;

            public FramebufferAttachment(FramebufferAttachmentConfiguration attachmentConfiguration)
            {
                textureName = attachmentConfiguration.TextureName;
                textureWidth = attachmentConfiguration.Width;
                textureHeight = attachmentConfiguration.Height;
                attachment = attachmentConfiguration.Attachment;
                internalFormat = attachmentConfiguration.InternalFormat;
                format = attachmentConfiguration.Format;
                type = attachmentConfiguration.Type;
                clearColor = attachmentConfiguration.ClearColor;
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

            public void Load(ShaderWindow window, out int width, out int height)
            {
                if (!window.Textures.TryGetValue(textureName, out texture))
                {
                    throw new InvalidOperationException(string.Format(
                        "The texture reference \"{0}\" was not found.",
                        textureName));
                }

                width = textureWidth.GetValueOrDefault(window.Width);
                height = textureHeight.GetValueOrDefault(window.Height);
                ClearTexture(texture.Id, width, height);
            }

            public void Attach()
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2D, texture.Id, 0);
            }

            public void Clear()
            {
                if (clearColor.HasValue)
                {
                    GL.ClearColor(clearColor.Value);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                }
            }

            public void Unload(ShaderWindow window)
            {
                texture = null;
            }
        }
    }
}
