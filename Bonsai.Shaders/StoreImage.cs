using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Writes the input image data to a texture.")]
    public class StoreImage : Combinator<IplImage, Texture>
    {
        readonly Texture2D configuration = new Texture2D();

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

        public override IObservable<Texture> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var texture = default(Texture);
                var textureSize = default(Size);
                return source.CombineEither(
                    ShaderManager.WindowUpdate(window =>
                    {
                        texture = configuration.CreateResource(window.ResourceManager);
                    }),
                    (input, window) =>
                    {
                        window.Update(() =>
                        {
                            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
                            var internalFormat = textureSize != input.Size ? InternalFormat : (PixelInternalFormat?)null;
                            TextureHelper.UpdateTexture(TextureTarget.Texture2D, internalFormat, input);
                            textureSize = input.Size;
                        });
                        return texture;
                    }).Finally(() =>
                    {
                        if (texture != null)
                        {
                            texture.Dispose();
                        }
                    });
            });
        }
    }
}
