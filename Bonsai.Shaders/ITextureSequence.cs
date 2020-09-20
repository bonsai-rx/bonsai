using System.Collections.Generic;

namespace Bonsai.Shaders
{
    interface ITextureSequence
    {
        double PlaybackRate { get; set; }

        IEnumerator<int> GetEnumerator(bool loop);
    }
}
