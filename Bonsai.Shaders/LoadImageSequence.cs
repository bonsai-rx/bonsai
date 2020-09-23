using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [DefaultProperty(nameof(FileName))]
    [Description("Loads an image sequence from the specified movie file or image folder.")]
    public class LoadImageSequence : Source<Texture>
    {
        readonly UpdateFrame updateFrame = new UpdateFrame();
        readonly ImageSequence configuration = new ImageSequence();

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
        [Description("The path to a movie file or image sequence search pattern.")]
        public string FileName
        {
            get { return configuration.FileName; }
            set { configuration.FileName = value; }
        }

        [Description("Specifies the optional flip mode applied to individual frames.")]
        public FlipMode? FlipMode
        {
            get { return configuration.FlipMode; }
            set { configuration.FlipMode = value; }
        }

        [Description("The optional maximum number of frames to include in the image sequence.")]
        public int? FrameCount
        {
            get { return configuration.FrameCount; }
            set { configuration.FrameCount = value; }
        }

        [Description("The offset, in frames, at which the image sequence should start.")]
        public int StartPosition
        {
            get { return configuration.StartPosition; }
            set { configuration.StartPosition = value; }
        }

        public override IObservable<Texture> Generate()
        {
            var update = updateFrame.Generate().Take(1);
            return update.Select(x => configuration.CreateResource(((ShaderWindow)x.Sender).ResourceManager));
        }

        public IObservable<Texture> Generate<TSource>(IObservable<TSource> source)
        {
            return source.SelectMany(Generate());
        }
    }
}
