using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for initializing streaming
    /// texture sequences from a movie file.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class VideoTexture : ImageSequence
    {
        /// <summary>
        /// Gets or sets the size of the pre-loading buffer for video frames.
        /// </summary>
        [Category("TextureData")]
        [Description("The size of the pre-loading buffer for video frames.")]
        public int? BufferLength { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="Texture"/> class providing
        /// support for streaming texture data from a movie file.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Texture"/> class representing
        /// the video texture.
        /// </returns>
        /// <inheritdoc/>
        public override Texture CreateResource(ResourceManager resourceManager)
        {
            var frames = GetFrames(FileName, clone: true, out bool video, out PixelInternalFormat? internalFormat);
            if (!video)
            {
                throw new InvalidOperationException(string.Format(
                    "The image sequence path \"{1}\" cannot be used for the video texture \"{0}\".",
                    Name, FileName));
            }

            frames.Reset();
            var bufferLength = BufferLength.GetValueOrDefault(1);
            var sequence = new TextureStream(frames, internalFormat, bufferLength);
            ConfigureTexture(sequence, frames.Width, frames.Height);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            sequence.PlaybackRate = frames.PlaybackRate;
            return sequence;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var name = Name;
            var fileName = FileName;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else if (string.IsNullOrEmpty(fileName)) return name;
            else return $"{name} [Video: {fileName}]";
        }
    }
}
