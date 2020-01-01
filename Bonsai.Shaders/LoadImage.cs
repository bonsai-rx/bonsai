using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [DefaultProperty("FileName")]
    [Description("Loads a texture buffer from the specified file.")]
    public class LoadImage : Source<Texture>
    {
        readonly UpdateFrame updateFrame = new UpdateFrame();
        readonly ImageTexture configuration = new ImageTexture();

        public LoadImage()
        {
            ColorType = LoadImageFlags.Unchanged;
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
        [FileNameFilter("Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF Files (*.tif)|*.tif")]
        [Description("The name of the image file.")]
        public string FileName
        {
            get { return configuration.FileName; }
            set { configuration.FileName = value; }
        }

        [Description("Specifies optional conversions applied to the loaded image.")]
        public LoadImageFlags ColorType
        {
            get { return configuration.ColorType; }
            set { configuration.ColorType = value; }
        }

        [Description("Specifies the optional flip mode applied to the loaded image.")]
        public FlipMode? FlipMode
        {
            get { return configuration.FlipMode; }
            set { configuration.FlipMode = value; }
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
