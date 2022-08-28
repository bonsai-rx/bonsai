using OpenTK.Audio.OpenAL;
using System.ComponentModel;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that starts playing the specified set of audio sources.
    /// If a source is already playing, it will start from the beginning.
    /// </summary>
    [Description("Starts playing the specified set of audio sources. If a source is already playing, it will start from the beginning.")]
    public class PlaySource : UpdateSourceState
    {
        internal override void Update(int[] sources)
        {
            AL.SourcePlay(sources.Length, sources);
        }
    }
}
