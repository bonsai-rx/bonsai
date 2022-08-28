using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that updates the pixel store of the specified
    /// texture target from a sequence of images.
    /// </summary>
    [Description("Updates the pixel store of the specified texture target from a sequence of images.")]
    public class UpdateTexture : Sink<IplImage>
    {
        /// <summary>
        /// Gets or sets the name of the texture to update.
        /// </summary>
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture to update.")]
        public string TextureName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the texture target to update.
        /// </summary>
        [Description("Specifies the texture target to update.")]
        public TextureTarget TextureTarget { get; set; } = TextureTarget.Texture2D;

        /// <summary>
        /// Gets or sets a value specifying the internal storage format of the
        /// texture target.
        /// </summary>
        [Description("Specifies the internal storage format of the texture target.")]
        public PixelInternalFormat InternalFormat { get; set; } = PixelInternalFormat.Rgba;

        /// <summary>
        /// Updates the pixel store of the specified texture target from an
        /// observable sequence of images.
        /// </summary>
        /// <param name="source">
        /// The sequence of images used to update the texture target.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// pixel store of the specified texture target.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Create<IplImage>(observer =>
            {
                var name = TextureName;
                var texture = default(Texture);
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
                            try { texture = window.ResourceManager.Load<Texture>(name); }
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
                                GL.BindTexture(TextureTarget.TextureCubeMap, texture.Id);
                            }
                            else GL.BindTexture(target, texture.Id);
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
