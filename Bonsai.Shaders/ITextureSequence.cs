using Bonsai.Reactive;
using System.Collections.Generic;

namespace Bonsai.Shaders
{
    interface ITextureSequence
    {
        double PlaybackRate { get; set; }

        IEnumerator<ElementIndex<Texture>> GetEnumerator(bool loop);
    }
}
