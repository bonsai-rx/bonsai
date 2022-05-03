using System.Collections.ObjectModel;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object specifying the framebuffer object used
    /// for render to texture shader passes.
    /// </summary>
    public class FramebufferConfiguration
    {
        readonly Collection<FramebufferAttachmentConfiguration> framebufferAttachments = new Collection<FramebufferAttachmentConfiguration>();

        /// <summary>
        /// Gets the collection of configuration objects specifying the framebuffer
        /// attachment slots used to render the framebuffer.
        /// </summary>
        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebufferAttachments; }
        }
    }
}
