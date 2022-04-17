using Bonsai.Resources;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ImageTexture : Texture2D
    {
        [Category("TextureData")]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif;*.tiff;*.exr|PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg|TIFF Files|*.tif;*.tiff|EXR Files|*.exr|All Files|*.*")]
        [Description("The name of the image file.")]
        public string FileName { get; set; }

        [Category("TextureData")]
        [Description("Specifies optional conversions applied to the loaded image.")]
        public LoadImageFlags ColorType { get; set; } = LoadImageFlags.Unchanged;

        [Category("TextureData")]
        [Description("Specifies the optional flip mode applied to the loaded image.")]
        public FlipMode? FlipMode { get; set; } = OpenCV.Net.FlipMode.Vertical;

        public override Texture CreateResource(ResourceManager resourceManager)
        {
            var fileName = FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException(string.Format(
                    "A valid image file path was not specified for texture \"{0}\".",
                    Name));
            }

            var texture = base.CreateResource(resourceManager);
            var image = CV.LoadImage(fileName, ColorType);
            if (image == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Failed to load image texture \"{0}\" from the specified path: \"{1}\".",
                    Name, fileName));
            }

            var width = Width.GetValueOrDefault();
            var height = Height.GetValueOrDefault();
            if (width > 0 && height > 0 && (image.Width != width || image.Height != height))
            {
                var resized = new IplImage(new Size(width, height), image.Depth, image.Channels);
                CV.Resize(image, resized);
                image = resized;
            }

            var flipMode = FlipMode;
            if (flipMode.HasValue) CV.Flip(image, null, flipMode.Value);
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
            var internalFormat = width > 0 && height > 0 ? (PixelInternalFormat?)null : InternalFormat;
            TextureHelper.UpdateTexture(TextureTarget.Texture2D, internalFormat, image);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }

        public override string ToString()
        {
            var name = Name;
            var fileName = FileName;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else if (string.IsNullOrEmpty(fileName)) return name;
            else return $"{name} [{fileName}]";
        }
    }
}
