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
        Texture texture;

        public FramebufferAttachmentConfiguration()
        {
            ClearColor = Color.Transparent;
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
        public PixelInternalFormat Format { get; set; }

        [XmlIgnore]
        [Description("The optional color used to clear the framebuffer before rendering.")]
        public Color? ClearColor { get; set; }

        [Browsable(false)]
        [XmlElement("ClearColor")]
        public string ClearColorHtml
        {
            get
            {
                var color = ClearColor;
                if (color.HasValue) return ColorTranslator.ToHtml(color.Value);
                else return null;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) ClearColor = null;
                else ClearColor = ColorTranslator.FromHtml(value);
            }
        }

        void ClearTexture(int texture, int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                Format, width, height, 0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Load(Shader shader, out int width, out int height)
        {
            if (!shader.Window.Textures.TryGetValue(TextureName, out texture))
            {
                throw new InvalidOperationException(string.Format(
                    "The texture reference \"{0}\" was not found.",
                    TextureName));
            }

            width = Width.GetValueOrDefault(shader.Window.Width);
            height = Height.GetValueOrDefault(shader.Window.Height);
            ClearTexture(texture.Id, width, height);
        }

        public void Attach()
        {
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, Attachment, TextureTarget.Texture2D, texture.Id, 0);
        }

        public void Clear()
        {
            var color = ClearColor;
            if (color.HasValue)
            {
                GL.ClearColor(color.Value);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            }
        }

        public void Unload(Shader shader)
        {
            texture = null;
        }
    }
}
