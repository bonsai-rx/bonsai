using Bonsai.Reactive;
using System.Collections.Generic;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents a sequence of texture objects with an enumerator.
    /// </summary>
    public class TextureSequence : Texture, ITextureSequence
    {
        readonly TextureArray textures;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureSequence"/> class
        /// with the specified number of texture objects in the internal <see cref="TextureArray"/>.
        /// </summary>
        /// <param name="length">
        /// The total number of texture objects in the <see cref="TextureArray"/>.
        /// </param>
        public TextureSequence(int length)
            : base(0)
        {
            textures = new TextureArray(length);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureSequence"/> class
        /// with existing texture handles.
        /// </summary>
        /// <param name="textures">
        /// The texture handles.
        /// </param> 
        public TextureSequence(int[] textures)
        {
            this.textures = new TextureArray(textures);
        }

        /// <summary>
        /// Gets the <see cref="TextureArray"/> associated with this texture sequence.
        /// </summary>
        public TextureArray Textures
        {
            get { return textures; }
        }

        /// <summary>
        /// The target playback rate of the texture sequence.
        /// </summary>
        public double PlaybackRate { get; set; }

        /// <summary>
        /// Returns an enumerator that iterates through all texture objects in
        /// the sequence.
        /// </summary>
        /// <param name="loop">
        /// Boolean specifying whether the sequence should loop.
        /// </param> 
        /// <returns>
        /// An enumerator that can be used to iterate through the texture objects
        /// in the sequence.
        /// </returns>
        public IEnumerator<ElementIndex<Texture>> GetEnumerator(bool loop)
        {
            var index = 0;
            var texture = new TextureReference();
            try
            {
                while (true)
                {
                    if (index >= textures.Length)
                    {
                        if (loop) index = 0;
                        else yield break;
                    }

                    texture.Id = Id = textures[index];
                    yield return new ElementIndex<Texture>(texture, index++);
                }
            }
            finally { texture.Id = Id = 0; }
        }

        internal override void Dispose(bool disposing)
        {
            if (disposing)
            {
                textures.Dispose();
            }
        }

        class TextureReference : Texture
        {
            public TextureReference()
                : base(0)
            {
            }

            internal override void Dispose(bool disposing)
            {
            }
        }
    }
}
