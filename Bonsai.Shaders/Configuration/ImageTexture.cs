using Bonsai.Resources;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for initializing a
    /// two-dimensional texture resource from the specified image file.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ImageTexture : Texture2D
    {
        /// <summary>
        /// Gets or sets the name of the image file.
        /// </summary>
        [Category("TextureData")]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif;*.tiff;*.exr|PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg|TIFF Files|*.tif;*.tiff|EXR Files|*.exr|All Files|*.*")]
        [Description("The name of the image file.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the color type of the loaded image.
        /// </summary>
        [Category("TextureData")]
        [Description("Specifies the color type of the loaded image.")]
        public LoadImageFlags ColorType { get; set; } = LoadImageFlags.Unchanged;

        /// <summary>
        /// Gets or sets a value specifying the flip mode applied to the loaded image.
        /// </summary>
        [Category("TextureData")]
        [Description("Specifies the flip mode applied to the loaded image.")]
        public FlipMode? FlipMode { get; set; } = OpenCV.Net.FlipMode.Vertical;

        /// <summary>
        /// Creates a new two-dimensional texture resource from the specified
        /// image file.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Texture"/> class representing
        /// the 2D texture.
        /// </returns>
        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
