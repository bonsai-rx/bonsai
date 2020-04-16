using System.Collections.ObjectModel;

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
