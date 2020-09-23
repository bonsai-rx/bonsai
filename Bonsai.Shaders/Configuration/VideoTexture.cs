using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class VideoTexture : ImageSequence
    {
        [Category("TextureData")]
        [Description("The optional size of the pre-loading buffer for video frames.")]
        public int? BufferLength { get; set; }

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
    }
}
