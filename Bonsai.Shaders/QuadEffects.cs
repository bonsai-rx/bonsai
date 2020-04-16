using System;

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
