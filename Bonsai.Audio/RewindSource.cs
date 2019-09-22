using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
