using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object specifying the texture to attach in a
    /// framebuffer attachment slot for render to texture shader passes.
    /// </summary>
    public class FramebufferAttachmentConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FramebufferAttachmentConfiguration"/> class.
        /// </summary>
        public FramebufferAttachmentConfiguration()
        {
            ClearColor = Color.Black;
            ClearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
            InternalFormat = PixelInternalFormat.Rgba;
            Format = PixelFormat.Rgba;
            Type = PixelType.UnsignedByte;
        }

        /// <summary>
        /// Gets or sets the name of the texture to attach to the framebuffer.
        /// </summary>
        [Category("Reference")]
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture to attach to the framebuffer.")]
        public string TextureName { get; set; }

        /// <summary>
        /// Gets or sets the width of the framebuffer texture attachment. If no value
        /// is specified, the width of the render window will be used.
        /// </summary>
        [Category("TextureSize")]
        [Description("The width of the framebuffer texture attachment.")]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the framebuffer texture attachment. If no value
        /// is specified, the height of the render window will be used.
        /// </summary>
        [Category("TextureSize")]
        [Description("The height of the framebuffer texture attachment.")]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the framebuffer attachment slot.
        /// </summary>
        [Description("Specifies the framebuffer attachment slot.")]
        public FramebufferAttachment Attachment { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the internal pixel format of the
        /// framebuffer texture.
        /// </summary>
        [Description("Specifies the internal pixel format of the framebuffer texture.")]
        public PixelInternalFormat InternalFormat { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the pixel format of the framebuffer
        /// texture.
        /// </summary>
        [Description("Specifies the pixel format of the framebuffer texture.")]
        public PixelFormat Format { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the pixel type of the framebuffer
        /// texture.
        /// </summary>
        [Description("Specifies the pixel type of the framebuffer texture.")]
        public PixelType Type { get; set; }

        /// <summary>
        /// Gets or sets the color used to clear the framebuffer before rendering.
        /// </summary>
        [XmlIgnore]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; }

        /// <summary>
        /// Gets or sets an HTML representation of the clear color value for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(ClearColor))]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        /// <summary>
        /// Gets or sets a value specifying which buffers to clear before rendering.
        /// </summary>
        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask { get; set; }
    }
}
