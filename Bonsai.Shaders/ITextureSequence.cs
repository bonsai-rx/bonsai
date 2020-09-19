namespace Bonsai.Shaders
{
    interface ITextureSequence
    {
        double PlaybackRate { get; set; }

        bool MoveNext();

        void Reset();
    }
}
