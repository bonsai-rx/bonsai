using Bonsai.Resources;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ImageCubemap : Cubemap
    {
        const string FileNameFilter = "Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF Files (*.tif)|*.tif";

        public ImageCubemap()
        {
            ColorType = LoadImageFlags.Unchanged;
        }

        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the positive X direction.")]
        public string PositiveX { get; set; }

        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the negative X direction.")]
        public string NegativeX { get; set; }

        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the positive Y direction.")]
        public string PositiveY { get; set; }

        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the negative Y direction.")]
        public string NegativeY { get; set; }

        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the positive Z direction.")]
        public string PositiveZ { get; set; }

        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the negative Z direction.")]
        public string NegativeZ { get; set; }

        [Description("Specifies optional conversions applied to the loaded image.")]
        public LoadImageFlags ColorType { get; set; }

        [Description("Specifies the optional flip mode applied to the loaded image.")]
        public FlipMode? FlipMode { get; set; }

        void LoadRenderTarget(TextureTarget target, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException(string.Format(
                    "A valid image file path was not specified for texture \"{0}:{1}\".",
                    Name, target));
            }

            var image = CV.LoadImage(fileName, ColorType);
            if (image == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Failed to load texture target \"{0}:{1}\" from the specified path: \"{2}\".",
                    Name, target, fileName));
            }

            var faceSize = FaceSize.GetValueOrDefault();
            if (faceSize > 0 && (image.Width != faceSize || image.Height != faceSize))
            {
                var resized = new IplImage(new Size(faceSize, faceSize), image.Depth, image.Channels);
                CV.Resize(image, resized);
                image = resized;
            }

            var flipMode = FlipMode;
            if (flipMode.HasValue) CV.Flip(image, null, flipMode.Value);
            var internalFormat = faceSize > 0 ? (PixelInternalFormat?)null : InternalFormat;
            TextureHelper.UpdateTexture(target, internalFormat, image);
        }

        public override Texture CreateResource(ResourceManager resourceManager)
        {
            var texture = base.CreateResource(resourceManager);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture.Id);
            LoadRenderTarget(TextureTarget.TextureCubeMapPositiveX, PositiveX);
            LoadRenderTarget(TextureTarget.TextureCubeMapNegativeX, NegativeX);
            LoadRenderTarget(TextureTarget.TextureCubeMapPositiveY, PositiveY);
            LoadRenderTarget(TextureTarget.TextureCubeMapNegativeY, NegativeY);
            LoadRenderTarget(TextureTarget.TextureCubeMapPositiveZ, PositiveZ);
            LoadRenderTarget(TextureTarget.TextureCubeMapNegativeZ, NegativeZ);
            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            return texture;
        }
    }
}
