using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    interface ITextureSequence
    {
        double PlaybackRate { get; set; }

        bool MoveNext();

        void Reset();
    }
}
