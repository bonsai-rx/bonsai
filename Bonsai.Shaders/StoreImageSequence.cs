using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that writes a sequence of images to a texture array.
    /// </summary>
    [Description("Writes a sequence of images to a texture array.")]
    public class StoreImageSequence : Combinator<IplImage[], Texture>
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
        /// Gets or sets the default rate at which to playback the stored
        /// image sequence.
        /// </summary>
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The default rate at which to playback the stored image sequence.")]
        public double PlaybackRate { get; set; }

        /// <summary>
        /// Writes each array of images in an observable sequence into a new
        /// texture array.
        /// </summary>
        /// <param name="source">
        /// A sequence of arrays of <see cref="IplImage"/> objects used to
        /// initialize the texture array.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Texture"/> objects where each texture
        /// stores the corresponding array of images in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public override IObservable<Texture> Process(IObservable<IplImage[]> source)
        {
            return source.SelectMany(input => ShaderManager.WindowUpdate().Select(window =>
            {
                var sequence = new TextureSequence(input.Length);
                using var enumerator = sequence.GetEnumerator(false);
                for (int i = 0; i < input.Length && enumerator.MoveNext(); i++)
                {
                    configuration.ConfigureTexture(sequence, input[i].Width, input[i].Height);
                    TextureHelper.UpdateTexture(TextureTarget.Texture2D, InternalFormat, input[i]);
                }

                GL.BindTexture(TextureTarget.Texture2D, 0);
                sequence.PlaybackRate = PlaybackRate;
                return sequence;
            }));
        }

        /// <summary>
        /// Writes an observable sequence of images to a texture array.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to be stored in the texture array.
        /// </param>
        /// <returns>
        /// An observable sequence containing the <see cref="Texture"/> object
        /// used to store all the images in the <paramref name="source"/> sequence.
        /// The initialized texture array is returned only when the image sequence
        /// is completed.
        /// </returns>
        public IObservable<Texture> Process(IObservable<IplImage> source)
        {
            return source.SelectMany(input => ShaderManager.WindowUpdate().Select(window =>
            {
                using var texture = new Texture();
                configuration.ConfigureTexture(texture, input.Width, input.Height);
                TextureHelper.UpdateTexture(TextureTarget.Texture2D, InternalFormat, input);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                var id = texture.Id;
                texture.Id = 0;
                return id;
            })).ToArray().Select(textures => new TextureSequence(textures)
            {
                PlaybackRate = PlaybackRate
            });
        }
    }
}
