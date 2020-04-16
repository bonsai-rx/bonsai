using OpenTK.Audio.OpenAL;
using System.ComponentModel;

namespace Bonsai.Audio
{
    [Description("Stops the specified set of audio sources.")]
    public class StopSource : UpdateSourceState
    {
        internal override void Update(int[] sources)
        {
            AL.SourceStop(sources.Length, sources);
        }
    }
}
