using OpenTK.Graphics;
using System.ComponentModel;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for defining the format for all graphics
    /// operations in the graphics context.
    /// </summary>
    public class GraphicsModeConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the color format of the color buffer.
        /// </summary>
        [Description("Specifies the color format of the color buffer.")]
        public ColorFormatConfiguration ColorFormat { get; set; } = new ColorFormatConfiguration(32);

        /// <summary>
        /// Gets or sets the number of bits in the depth buffer.
        /// </summary>
        [Description("The number of bits in the depth buffer.")]
        public int Depth { get; set; } = 16;

        /// <summary>
        /// Gets or sets the number of bits in the stencil buffer.
        /// </summary>
        [Description("The number of bits in the stencil buffer.")]
        public int Stencil { get; set; }

        /// <summary>
        /// Gets or sets the number of samples to use for full screen anti-aliasing.
        /// </summary>
        [Description("The number of samples to use for full screen anti-aliasing.")]
        public int Samples { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the color format of the accumulator buffer.
        /// </summary>
        [Description("Specifies the color format of the accumulator buffer.")]
        public ColorFormatConfiguration AccumulatorFormat { get; set; } = new ColorFormatConfiguration(0);

        /// <summary>
        /// Gets or sets the number of render buffers. Typical values include
        /// one (single-), two (double-) or three (triple-buffering).
        /// </summary>
        [Description("The number of render buffers. Typical values include one (single-), two (double-) or three (triple-buffering).")]
        public int Buffers { get; set; } = 2;

        /// <summary>
        /// Gets or sets a value specifying whether to create a graphics mode with
        /// stereo capabilities.
        /// </summary>
        [Description("Specifies whether to create a graphics mode with stereo capabilities.")]
        public bool Stereo { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="GraphicsMode"/> class specifying
        /// the format for graphics operations.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="GraphicsMode"/> class using the specified
        /// format properties of this configuration object.
        /// </returns>
        public GraphicsMode CreateGraphicsMode()
        {
            var color = ColorFormat;
            var accum = AccumulatorFormat;
            return new GraphicsMode(
                new ColorFormat(color.Red, color.Green, color.Blue, color.Alpha),
                Depth, Stencil, Samples,
                new ColorFormat(accum.Red, accum.Green, accum.Blue, accum.Alpha),
                Buffers, Stereo);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{{Color: {0}, Depth: {1}, Stencil: {2}, Samples: {3}, Accumulator: {4}, Buffers: {5}, Stereo: {6}}}",
                ColorFormat,
                Depth,
                Stencil,
                Samples,
                AccumulatorFormat,
                Buffers,
                Stereo);
        }
    }
}
