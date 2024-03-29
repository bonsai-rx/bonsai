﻿using Bonsai.Resources;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for initializing cubemap
    /// texture resources from the specified image files.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ImageCubemap : Cubemap
    {
        const string FileNameFilter = "Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF Files (*.tif)|*.tif";

        /// <summary>
        /// Gets or sets the name of the image file to load for the positive
        /// X direction.
        /// </summary>
        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the positive X direction.")]
        public string PositiveX { get; set; }

        /// <summary>
        /// Gets or sets the name of the image file to load for the negative
        /// X direction.
        /// </summary>
        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the negative X direction.")]
        public string NegativeX { get; set; }

        /// <summary>
        /// Gets or sets the name of the image file to load for the positive
        /// Y direction.
        /// </summary>
        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the positive Y direction.")]
        public string PositiveY { get; set; }

        /// <summary>
        /// Gets or sets the name of the image file to load for the negative
        /// Y direction.
        /// </summary>
        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the negative Y direction.")]
        public string NegativeY { get; set; }

        /// <summary>
        /// Gets or sets the name of the image file to load for the positive
        /// Z direction.
        /// </summary>
        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the positive Z direction.")]
        public string PositiveZ { get; set; }

        /// <summary>
        /// Gets or sets the name of the image file to load for the negative
        /// Z direction.
        /// </summary>
        [Category("TextureData")]
        [FileNameFilter(FileNameFilter)]
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the image file to load for the negative Z direction.")]
        public string NegativeZ { get; set; }

        /// <summary>
        /// Gets or sets a value specifying optional conversions applied to the
        /// loaded image.
        /// </summary>
        [Category("TextureData")]
        [Description("Specifies optional conversions applied to the loaded image.")]
        public LoadImageFlags ColorType { get; set; } = LoadImageFlags.Unchanged;

        /// <summary>
        /// Gets or sets a value specifying the optional flip mode applied to the
        /// loaded image.
        /// </summary>
        [Category("TextureData")]
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

        /// <summary>
        /// Creates a new cubemap texture resource from the specified image files.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Texture"/> class representing
        /// the cubemap texture.
        /// </returns>
        /// <inheritdoc/>
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
