using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    public class FramebufferAttachmentConfiguration
    {
        public FramebufferAttachmentConfiguration()
        {
            ClearColor = Color.Black;
            ClearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
            InternalFormat = PixelInternalFormat.Rgba;
            Format = PixelFormat.Rgba;
            Type = PixelType.UnsignedByte;
        }

        [Category("Reference")]
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture that will be attached to the framebuffer.")]
        public string TextureName { get; set; }

        [Category("TextureSize")]
        [Description("The optional width of the framebuffer texture attachment.")]
        public int? Width { get; set; }

        [Category("TextureSize")]
        [Description("The optional height of the framebuffer texture attachment.")]
        public int? Height { get; set; }

        [Description("The framebuffer attachment slot.")]
        public FramebufferAttachment Attachment { get; set; }

        [Description("The internal pixel format of the framebuffer texture.")]
        public PixelInternalFormat InternalFormat { get; set; }

        [Description("The pixel format of the framebuffer texture.")]
        public PixelFormat Format { get; set; }

        [Description("The pixel type of the framebuffer texture.")]
        public PixelType Type { get; set; }

        [XmlIgnore]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; }

        [Browsable(false)]
        [XmlElement("ClearColor")]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask { get; set; }
    }
}
