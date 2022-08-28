using OpenTK.Audio.OpenAL;
using System.ComponentModel;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that rewinds the specified set of audio sources
    /// back to the initial state.
    /// </summary>
    [Description("Rewinds the specified set of audio sources back to the initial state.")]
    public class RewindSource : UpdateSourceState
    {
        internal override void Update(int[] sources)
        {
            AL.SourceRewind(sources.Length, sources);
        }
    }
}
