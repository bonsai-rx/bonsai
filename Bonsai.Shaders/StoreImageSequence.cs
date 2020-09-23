using Bonsai.Shaders.Configuration;
using OpenCV.Net;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Description("Writes the input image sequence to a texture array.")]
    public class StoreImageSequence : Combinator<IplImage[], Texture>
    {
        readonly Texture2D configuration = new Texture2D();

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

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The default rate at which to playback the stored image sequence.")]
        public double PlaybackRate { get; set; }

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
