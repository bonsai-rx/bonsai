using OpenTK.Audio.OpenAL;
using System.ComponentModel;

namespace Bonsai.Audio
{
    [Description("Rewinds the specified set of audio sources back to the initial state.")]
    public class RewindSource : UpdateSourceState
    {
        internal override void Update(int[] sources)
        {
            AL.SourceRewind(sources.Length, sources);
        }
    }
}
