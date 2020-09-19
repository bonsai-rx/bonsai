namespace Bonsai.Shaders
{
    class TextureSequence : TextureArray, ITextureSequence
    {
        int index;

        public TextureSequence(int bufferLength)
            : base(bufferLength)
        {
        }

        public bool Loop { get; set; }

        public double PlaybackRate { get; set; }

        public bool MoveNext()
        {
            if (index >= Length)
            {
                if (Loop) index = 0;
                else return false;
            }

            SetActiveTexture(index);
            index = index + 1;
            return true;
        }

        public void Reset()
        {
            index = 0;
            Id = 0;
        }
    }
}
