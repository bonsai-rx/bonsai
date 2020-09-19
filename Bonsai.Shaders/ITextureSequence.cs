namespace Bonsai.Shaders
{
    interface ITextureSequence
    {
        bool Loop { get; set; }

        double PlaybackRate { get; set; }

        bool MoveNext();

        void Reset();
    }
}
