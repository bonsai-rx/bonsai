using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Updates the pixel store of the specified texture.")]
    public class UpdateTexture : Sink<IplImage>
    {
        public UpdateTexture()
        {
            InternalFormat = PixelInternalFormat.Rgba;
        }

        [Description("The name of the texture to update.")]
        [Editor("Bonsai.Shaders.Configuration.Design.TextureConfigurationEditor, Bonsai.Shaders.Design", typeof(UITypeEditor))]
        public string TextureName { get; set; }

        [Description("The internal storage format of the texture data.")]
        public PixelInternalFormat InternalFormat { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Create<IplImage>(observer =>
            {
                var texture = 0;
                var name = TextureName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A texture sampler name must be specified.");
                }

                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            Texture tex;
                            if (!window.Textures.TryGetValue(name, out tex))
                            {
                                observer.OnError(new InvalidOperationException(string.Format(
                                    "The texture \"{0}\" was not found.",
                                    name)));
                                return;
                            }

                            texture = tex.Id;
                        });
                    }),
                    (input, window) =>
                    {
                        window.Update(() =>
                        {
                            TextureHelper.UpdateTexture(texture, InternalFormat, input);
                        });
                        return input;
                    }).SubscribeSafe(observer);
            });
        }
    }
}
