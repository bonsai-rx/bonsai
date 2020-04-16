using OpenTK.Audio.OpenAL;
using System.ComponentModel;

namespace Bonsai.Audio
{
    [Description("Pauses the specified set of audio sources.")]
    public class PauseSource : UpdateSourceState
    {
        internal override void Update(int[] sources)
        {
            AL.SourcePause(sources.Length, sources);
        }
    }
}
