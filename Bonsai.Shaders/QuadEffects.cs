using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Flags]
    public enum QuadEffects
    {
        None = 0,
        FlipHorizontally = 1,
        FlipVertically = 2,
        FlipBoth = 3
    }
}
