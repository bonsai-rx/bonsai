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
    internal class ShaderState
    {
        readonly Shader shader;
        readonly List<StateConfiguration> renderState;
        readonly List<TextureBinding> textureBindings;
        readonly Framebuffer framebuffer;

        public ShaderState(
            Shader shaderTarget,
            IEnumerable<StateConfiguration> renderStateConfiguration,
            IEnumerable<TextureBindingConfiguration> textureBindingConfiguration,
            FramebufferConfiguration framebufferConfiguration)
        {
            if (shaderTarget == null)
            {
                throw new ArgumentNullException("shaderTarget");
            }

            if (renderStateConfiguration == null)
            {
                throw new ArgumentNullException("renderStateConfiguration");
            }

            if (textureBindingConfiguration == null)
            {
                throw new ArgumentNullException("textureBindingConfiguration");
            }

            if (framebufferConfiguration == null)
            {
                throw new ArgumentNullException("framebufferConfiguration");
            }

            shader = shaderTarget;
            renderState = renderStateConfiguration.ToList();
            textureBindings = textureBindingConfiguration.Select(configuration => new TextureBinding(configuration)).ToList();
            framebuffer = new Framebuffer(framebufferConfiguration);
        }
        
        public void Load()
        {
            foreach (var binding in textureBindings)
            {
                binding.Load(shader);
            }

            framebuffer.Load(shader);
        }

        public void Bind()
        {
            foreach (var state in renderState)
            {
                state.Execute(shader.Window);
            }

            foreach (var binding in textureBindings)
            {
                binding.Bind(shader);
            }

            framebuffer.Bind(shader);
        }

        public void Unbind()
        {
            framebuffer.Unbind(shader);
            foreach (var binding in textureBindings)
            {
                binding.Unbind(shader);
            }
        }

        public void Unload()
        {
            framebuffer.Unload(shader);
            foreach (var binding in textureBindings)
            {
                binding.Unload(shader);
            }
        }

        class Framebuffer
        {
            readonly List<FramebufferAttachment> framebufferAttachments;
            int framebufferWidth;
            int framebufferHeight;
            int fbo;

            public Framebuffer(FramebufferConfiguration framebufferConfiguration)
            {
                if (framebufferConfiguration == null)
                {
                    throw new ArgumentNullException("framebufferConfiguration");
                }

                framebufferAttachments = framebufferConfiguration.FramebufferAttachments
                                                                 .Select(configuration => new FramebufferAttachment(configuration))
                                                                 .ToList();
            }

            public void Load(Shader shader)
            {
                framebufferWidth = 0;
                framebufferHeight = 0;
                if (framebufferAttachments.Count > 0)
                {
                    foreach (var attachment in framebufferAttachments)
                    {
                        int width, height;
                        attachment.Load(shader, out width, out height);
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

            public void Bind(Shader shader)
            {
                if (fbo > 0)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                    foreach (var attachment in framebufferAttachments)
                    {
                        attachment.Clear();
                    }

                    shader.Window.UpdateViewport(framebufferWidth, framebufferHeight);
                }
            }

            public void Unbind(Shader shader)
            {
                if (fbo > 0)
                {
                    shader.Window.UpdateViewport();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                }
            }

            public void Unload(Shader shader)
            {
                if (fbo > 0)
                {
                    foreach (var attachment in framebufferAttachments)
                    {
                        attachment.Unload(shader);
                    }

                    framebufferWidth = 0;
                    framebufferHeight = 0;
                    GL.DeleteFramebuffers(1, ref fbo);
                }
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

            public void Load(Shader shader, out int width, out int height)
            {
                if (!shader.Window.Textures.TryGetValue(textureName, out texture))
                {
                    throw new InvalidOperationException(string.Format(
                        "The texture reference \"{0}\" was not found.",
                        textureName));
                }

                width = textureWidth.GetValueOrDefault(shader.Window.Width);
                height = textureHeight.GetValueOrDefault(shader.Window.Height);
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

            public void Unload(Shader shader)
            {
                texture = null;
            }
        }

        class TextureBinding
        {
            Texture texture;
            readonly string name;
            readonly string textureName;
            readonly TextureUnit textureSlot;

            public TextureBinding(TextureBindingConfiguration bindingConfiguration)
            {
                name = bindingConfiguration.Name;
                textureName = bindingConfiguration.TextureName;
                textureSlot = bindingConfiguration.TextureSlot;
            }

            public void Load(Shader shader)
            {
                shader.SetTextureSlot(name, textureSlot);
                if (!shader.Window.Textures.TryGetValue(textureName, out texture))
                {
                    throw new InvalidOperationException(string.Format(
                        "The texture reference \"{0}\" was not found.",
                        textureName));
                }
            }

            public void Bind(Shader shader)
            {
                GL.ActiveTexture(textureSlot);
                GL.BindTexture(TextureTarget.Texture2D, texture.Id);
            }

            public void Unbind(Shader shader)
            {
                GL.ActiveTexture(textureSlot);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            public void Unload(Shader shader)
            {
                texture = null;
            }
        }
    }
}
