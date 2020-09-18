using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [DefaultProperty("FileName")]
    [Description("Loads a texture sequence from the specified movie file.")]
    public class LoadVideo : Source<Texture>
    {
        readonly UpdateFrame updateFrame = new UpdateFrame();
        readonly VideoTexture configuration = new VideoTexture();

        public LoadVideo()
        {
            FlipMode = OpenCV.Net.FlipMode.Vertical;
        }

        [Category("TextureSize")]
        [Description("The optional width of the texture.")]
        public int? Width
        {
            get { return configuration.Width; }
            set { configuration.Width = value; }
        }

        [Category("TextureSize")]
        [Description("The optional height of the texture.")]
        public int? Height
        {
            get { return configuration.Height; }
            set { configuration.Height = value; }
        }

        [Category("TextureParameter")]
        [Description("The internal pixel format of the texture.")]
        public PixelInternalFormat InternalFormat
        {
            get { return configuration.InternalFormat; }
            set { configuration.InternalFormat = value; }
        }

        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the column coordinates of the texture sampler.")]
        public TextureWrapMode WrapS
        {
            get { return configuration.WrapS; }
            set { configuration.WrapS = value; }
        }

        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the row coordinates of the texture sampler.")]
        public TextureWrapMode WrapT
        {
            get { return configuration.WrapT; }
            set { configuration.WrapT = value; }
        }

        [Category("TextureParameter")]
        [Description("Specifies the texture minification filter.")]
        public TextureMinFilter MinFilter
        {
            get { return configuration.MinFilter; }
            set { configuration.MinFilter = value; }
        }

        [Category("TextureParameter")]
        [Description("Specifies the texture magnification filter.")]
        public TextureMagFilter MagFilter
        {
            get { return configuration.MagFilter; }
            set { configuration.MagFilter = value; }
        }

        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("Video Files|*.avi;*.mp4;*.ogg;*.ogv;*.wmv|AVI Files (*.avi)|*.avi|MP4 Files (*.mp4)|*.mp4|OGG Files (*.ogg;*.ogv)|*.ogg;*.ogv|WMV Files (*.wmv)|*.wmv")]
        [Description("The name of the movie file.")]
        public string FileName
        {
            get { return configuration.FileName; }
            set { configuration.FileName = value; }
        }

        [Description("Specifies the optional flip mode applied to loaded video frames.")]
        public FlipMode? FlipMode
        {
            get { return configuration.FlipMode; }
            set { configuration.FlipMode = value; }
        }

        [Description("The optional size of the pre-loading buffer for video frames.")]
        public int? BufferLength
        {
            get { return configuration.BufferLength; }
            set { configuration.BufferLength = value; }
        }

        [Description("The optional offset into the video, in frames, at which the sequence should start.")]
        public int Offset
        {
            get { return configuration.Offset; }
            set { configuration.Offset = value; }
        }

        [Description("Indicates whether the video should loop when the end of the file is reached.")]
        public bool Loop
        {
            get { return configuration.Loop; }
            set { configuration.Loop = value; }
        }

        [Description("Indicates whether to preload video frames into texture memory. This parameter is ignored if the number of frames exceeds the buffer size.")]
        public bool Preload
        {
            get { return configuration.Preload; }
            set { configuration.Preload = value; }
        }

        public override IObservable<Texture> Generate()
        {
            var update = updateFrame.Generate().Take(1);
            return update.Select(x => configuration.CreateResource(((ShaderWindow)x.Sender).ResourceManager));
        }

        public IObservable<Texture> Generate<TSource>(IObservable<double> source)
        {
            return source.SelectMany(Generate());
        }
    }
}
