using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
