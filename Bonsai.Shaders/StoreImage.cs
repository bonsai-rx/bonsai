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
        readonly UpdateFrame updateFrame = new UpdateFrame();
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

        async Task<IDisposable> CreateTexture(IObservable<IplImage> source, IObserver<Texture> observer)
        {
            var evt = await updateFrame.Generate().FirstOrDefaultAsync();
            if (evt != null)
            {
                var textureSize = default(Size);
                var window = (ShaderWindow)evt.Sender;
                var texture = configuration.CreateResource(window.ResourceManager);
                var update = Observer.Create<IplImage>(input =>
                {
                    window.Update(() =>
                    {
                        GL.BindTexture(TextureTarget.Texture2D, texture.Id);
                        var internalFormat = textureSize != input.Size ? InternalFormat : (PixelInternalFormat?)null;
                        TextureHelper.UpdateTexture(TextureTarget.Texture2D, texture.Id, internalFormat, input);
                        textureSize = input.Size;
                        observer.OnNext(texture);
                    });
                });
                var windowClosed = window.EventPattern<EventArgs>(
                    handler => window.Closed += handler,
                    handler => window.Closed -= handler);
                return source.TakeUntil(windowClosed).Finally(() =>
                {
                    window.Update(texture.Dispose);
                }).SubscribeSafe(update);
            }
            else return Disposable.Empty;
        }

        public override IObservable<Texture> Process(IObservable<IplImage> source)
        {
            return Observable.Create<Texture>(observer => CreateTexture(source, observer));
        }
    }
}
