using System.Collections.Generic;

namespace Bonsai.Shaders
{
    class TextureSequence : Texture, ITextureSequence
    {
        readonly TextureArray textures;

        public TextureSequence(int bufferLength)
            : base(0)
        {
            textures = new TextureArray(bufferLength);
        }

        public TextureArray Textures
        {
            get { return textures; }
        }

        public double PlaybackRate { get; set; }

        public IEnumerator<int> GetEnumerator(bool loop)
        {
            var index = 0;
            try
            {
                while (true)
                {
                    if (index >= textures.Length)
                    {
                        if (loop) index = 0;
                        else yield break;
                    }

                    Id = textures[index];
                    yield return index++;
                }
            }
            finally { Id = 0; }
        }

        internal override void Dispose(bool disposing)
        {
            if (disposing)
            {
                textures.Dispose();
            }
        }
    }
}
