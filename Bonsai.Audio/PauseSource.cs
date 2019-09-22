using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
