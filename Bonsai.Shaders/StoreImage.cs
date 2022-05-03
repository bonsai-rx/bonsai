using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that writes each image in the sequence to a
    /// texture object.
    /// </summary>
    [Description("Writes each image in the sequence to a texture object.")]
    public class StoreImage : Combinator<IplImage, Texture>
    {
        readonly Texture2D configuration = new Texture2D();

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
        /// Writes each image in an observable sequence to a texture object.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to write into the texture.
        /// </param>
        /// <returns>
        /// An observable sequence returning the <see cref="Texture"/> object
        /// on which each image is stored, whenever the <paramref name="source"/>
        /// sequence emits a new image.
        /// </returns>
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
