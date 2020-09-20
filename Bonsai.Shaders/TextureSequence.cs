namespace Bonsai.Shaders
{
    class TextureSequence : Texture, ITextureSequence
    {
        int index;
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

        public bool Loop { get; set; }

        public double PlaybackRate { get; set; }

        public bool MoveNext()
        {
            if (index >= textures.Length)
            {
                if (Loop) index = 0;
                else return false;
            }

            Id = textures[index++];
            return true;
        }

        public void Reset()
        {
            index = 0;
            Id = 0;
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
