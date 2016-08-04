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

        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebufferAttachments; }
        }
    }
}
