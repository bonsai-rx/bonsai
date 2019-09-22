using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    [Description("Starts playing the specified set of audio sources. If a source is already playing, it will start from the beginning.")]
    public class PlaySource : UpdateSourceState
    {
        internal override void Update(int[] sources)
        {
            AL.SourcePlay(sources.Length, sources);
        }
    }
}
