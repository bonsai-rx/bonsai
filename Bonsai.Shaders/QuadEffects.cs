using System;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Specifies available fullscreen quad rendering effects.
    /// </summary>
    [Flags]
    public enum QuadEffects
    {
        /// <summary>
        /// Specifies the quad should not be flipped.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies the quad should be flipped horizontally.
        /// </summary>
        FlipHorizontally = 1,

        /// <summary>
        /// Specifies the quad should be flipped vertically.
        /// </summary>
        FlipVertically = 2,

        /// <summary>
        /// Specifies the quad should be flipped both vertically and horizontally.
        /// </summary>
        FlipBoth = 3
    }
}
