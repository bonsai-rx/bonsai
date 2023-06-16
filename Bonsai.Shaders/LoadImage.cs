using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that loads a texture buffer from the specified image file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Loads a texture buffer from the specified image file.")]
    public class LoadImage : Source<Texture>
    {
        readonly UpdateFrame updateFrame = new UpdateFrame();
        readonly ImageTexture configuration = new ImageTexture();

        /// <summary>
        /// Gets or sets the width of the texture. If no value is specified, the
        /// texture buffer will not be initialized.
        /// </summary>
        [Category("TextureSize")]
        [Description("The width of the texture. If no value is specified, the texture buffer will not be initialized.")]
        public int? Width
        {
            get { return configuration.Width; }
            set { configuration.Width = value; }
        }

        /// <summary>
        /// Gets or sets the height of the texture. If no value is specified, the
        /// texture buffer will not be initialized.
        /// </summary>
        [Category("TextureSize")]
        [Description("The height of the texture. If no value is specified, the texture buffer will not be initialized.")]
        public int? Height
        {
            get { return configuration.Height; }
            set { configuration.Height = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the internal pixel format of the texture.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the internal pixel format of the texture.")]
        public PixelInternalFormat InternalFormat
        {
            get { return configuration.InternalFormat; }
            set { configuration.InternalFormat = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying wrapping parameters for the column
        /// coordinates of the texture sampler.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the column coordinates of the texture sampler.")]
        public TextureWrapMode WrapS
        {
            get { return configuration.WrapS; }
            set { configuration.WrapS = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying wrapping parameters for the row
        /// coordinates of the texture sampler.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the row coordinates of the texture sampler.")]
        public TextureWrapMode WrapT
        {
            get { return configuration.WrapT; }
            set { configuration.WrapT = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the texture minification filter.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the texture minification filter.")]
        public TextureMinFilter MinFilter
        {
            get { return configuration.MinFilter; }
            set { configuration.MinFilter = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the texture magnification filter.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the texture magnification filter.")]
        public TextureMagFilter MagFilter
        {
            get { return configuration.MagFilter; }
            set { configuration.MagFilter = value; }
        }

        /// <summary>
        /// Gets or sets the name of the image file.
        /// </summary>
        [Category("TextureData")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif;*.tiff;*.exr|PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg|TIFF Files|*.tif;*.tiff|EXR Files|*.exr|All Files|*.*")]
        [Description("The name of the image file.")]
        public string FileName
        {
            get { return configuration.FileName; }
            set { configuration.FileName = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the color type of the loaded image.
        /// </summary>
        [Category("TextureData")]
        [Description("Specifies the color type of the loaded image.")]
        public LoadImageFlags ColorType
        {
            get { return configuration.ColorType; }
            set { configuration.ColorType = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the flip mode applied to the loaded image.
        /// </summary>
        [Category("TextureData")]
        [Description("Specifies the flip mode applied to the loaded image.")]
        public FlipMode? FlipMode
        {
            get { return configuration.FlipMode; }
            set { configuration.FlipMode = value; }
        }

        ImageTexture CloneImageConfiguration()
        {
            return new ImageTexture
            {
                Width = configuration.Width,
                Height = configuration.Height,
                InternalFormat = configuration.InternalFormat,
                WrapS = configuration.WrapS,
                WrapT = configuration.WrapT,
                MinFilter = configuration.MinFilter,
                MagFilter = configuration.MagFilter,
                FileName = configuration.FileName,
                ColorType = configuration.ColorType,
                FlipMode = configuration.FlipMode
            };
        }

        /// <summary>
        /// Generates an observable sequence that returns a texture buffer loaded
        /// from the specified image file.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="Texture"/>
        /// class representing the data loaded from the specified image file.
        /// </returns>
        public override IObservable<Texture> Generate()
        {
            var update = updateFrame.Generate().Take(1);
            var configuration = CloneImageConfiguration();
            return update.Select(x => configuration.CreateResource(((ShaderWindow)x.Sender).ResourceManager));
        }

        /// <summary>
        /// Returns a texture buffer loaded from the specified image file whenever
        /// an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to start loading a
        /// new texture buffer from the specified image file.
        /// </param>
        /// <returns>
        /// The sequence of <see cref="Texture"/> objects loaded from the
        /// specified image file whenever the <paramref name="source"/> sequence
        /// emits a notification.
        /// </returns>
        public IObservable<Texture> Generate<TSource>(IObservable<TSource> source)
        {
            var update = updateFrame.Generate();
            return ShaderManager.WindowSource.Take(1).SelectMany(window =>
                source.Select(_ => CloneImageConfiguration())
                      .Buffer(update)
                      .SelectMany(xs => xs)
                      .Select(configuration => configuration.CreateResource(window.ResourceManager)));
        }
    }
}
