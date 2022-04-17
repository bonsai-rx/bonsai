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
    /// Represents an operator that initializes a video texture which is
    /// dynamically updated from the specified movie file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Initializes a video texture which is dynamically updated from the specified movie file.")]
    public class LoadVideo : Source<Texture>
    {
        readonly UpdateFrame updateFrame = new UpdateFrame();
        readonly VideoTexture configuration = new VideoTexture();

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
        /// Gets or sets the path to a movie file.
        /// </summary>
        [Category("TextureData")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("Video Files|*.avi;*.mp4;*.ogg;*.ogv;*.wmv|AVI Files (*.avi)|*.avi|MP4 Files (*.mp4)|*.mp4|OGG Files (*.ogg;*.ogv)|*.ogg;*.ogv|WMV Files (*.wmv)|*.wmv")]
        [Description("The path to a movie file.")]
        public string FileName
        {
            get { return configuration.FileName; }
            set { configuration.FileName = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the flip mode applied to individual frames.
        /// </summary>
        [Category("TextureData")]
        [Description("Specifies the flip mode applied to individual frames.")]
        public FlipMode? FlipMode
        {
            get { return configuration.FlipMode; }
            set { configuration.FlipMode = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of frames to include in the video sequence.
        /// </summary>
        /// <remarks>
        /// If no value is specified, all frames in the video will be played.
        /// </remarks>
        [Category("TextureData")]
        [Description("The maximum number of frames to include in the video sequence.")]
        public int? FrameCount
        {
            get { return configuration.FrameCount; }
            set { configuration.FrameCount = value; }
        }

        /// <summary>
        /// Gets or sets the offset, in frames, at which the video sequence should start.
        /// </summary>
        [Category("TextureData")]
        [Description("The offset, in frames, at which the video sequence should start.")]
        public int StartPosition
        {
            get { return configuration.StartPosition; }
            set { configuration.StartPosition = value; }
        }

        /// <summary>
        /// Gets or sets the size of the pre-loading buffer for video frames.
        /// </summary>
        [Category("TextureData")]
        [Description("The size of the pre-loading buffer for video frames.")]
        public int? BufferLength
        {
            get { return configuration.BufferLength; }
            set { configuration.BufferLength = value; }
        }

        /// <summary>
        /// Generates an observable sequence that returns a video texture
        /// initialized from the specified movie file.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="Texture"/>
        /// class representing the video texture.
        /// </returns>
        public override IObservable<Texture> Generate()
        {
            var update = updateFrame.Generate().Take(1);
            return update.Select(x => configuration.CreateResource(((ShaderWindow)x.Sender).ResourceManager));
        }

        /// <summary>
        /// Returns a video texture initialized from the specified movie file
        /// whenever an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to start loading a
        /// new video texture.
        /// </param>
        /// <returns>
        /// The sequence of <see cref="Texture"/> objects initialized from the
        /// specified movie file whenever the <paramref name="source"/> sequence
        /// emits a notification.
        /// </returns>
        public IObservable<Texture> Generate<TSource>(IObservable<TSource> source)
        {
            return source.SelectMany(Generate());
        }
    }
}
