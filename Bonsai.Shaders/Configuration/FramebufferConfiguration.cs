using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    public class FramebufferConfiguration
    {
        readonly Collection<FramebufferAttachmentConfiguration> framebufferAttachments = new Collection<FramebufferAttachmentConfiguration>();
        int framebufferWidth;
        int framebufferHeight;
        int fbo;

        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebufferAttachments; }
        }

        [Browsable(false)]
        public int Framebuffer
        {
            get { return fbo; }
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
                foreach (var attachment in framebufferAttachments)
                {
                    attachment.Clear();
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
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
}
