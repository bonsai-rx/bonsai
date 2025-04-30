using Bonsai.Reactive;
using System.Collections;
using System.Collections.Generic;

namespace Bonsai.Shaders
{
    class TextureSequence : Texture, ITextureSequence, ITextureArray
    {
        readonly TextureArray textures;

        public TextureSequence(int length)
            : base(0)
        {
            textures = new TextureArray(length);
        }

        public TextureSequence(int[] textures)
        {
            this.textures = new TextureArray(textures);
        }

        public TextureArray Textures
        {
            get { return textures; }
        }

        public double PlaybackRate { get; set; }

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

        #region ITextureArray Members

        int ITextureArray.Length => textures.Length;

        int ITextureArray.this[int index] => textures[index];

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return textures.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return textures.GetEnumerator();
        }

        #endregion
    }
}
