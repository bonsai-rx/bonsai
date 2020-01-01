using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Updates the pixel store of the specified texture target.")]
    public class UpdateTexture : Sink<IplImage>
    {
        public UpdateTexture()
        {
            TextureTarget = TextureTarget.Texture2D;
            InternalFormat = PixelInternalFormat.Rgba;
        }

        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture to update.")]
        public string TextureName { get; set; }

        [Description("The texture target to update.")]
        public TextureTarget TextureTarget { get; set; }

        [Description("The internal storage format of the texture target.")]
        public PixelInternalFormat InternalFormat { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Create<IplImage>(observer =>
            {
                var texture = 0;
                var name = TextureName;
                var textureSize = default(Size);
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A texture name must be specified.");
                }

                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            try
                            {
                                var tex = window.ResourceManager.Load<Texture>(name);
                                texture = tex.Id;
                            }
                            catch (Exception ex) { observer.OnError(ex); }
                        });
                    }),
                    (input, window) =>
                    {
                        window.Update(() =>
                        {
                            var target = TextureTarget;
                            if (target > TextureTarget.TextureBindingCubeMap && target < TextureTarget.ProxyTextureCubeMap)
                            {
                                GL.BindTexture(TextureTarget.TextureCubeMap, texture);
                            }
                            else GL.BindTexture(target, texture);
                            var internalFormat = textureSize != input.Size ? InternalFormat : (PixelInternalFormat?)null;
                            TextureHelper.UpdateTexture(target, internalFormat, input);
                            textureSize = input.Size;
                        });
                        return input;
                    }).SubscribeSafe(observer);
            });
        }
    }
}
