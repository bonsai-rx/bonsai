using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class GraphicsModeConfiguration
    {
        public GraphicsModeConfiguration()
        {
            ColorFormat = new ColorFormatConfiguration(32);
            AccumulatorFormat = new ColorFormatConfiguration(0);
            Depth = 16;
            Buffers = 2;
        }

        [Description("The color format of the color buffer.")]
        public ColorFormatConfiguration ColorFormat { get; set; }

        [Description("The number of bits in the depth buffer.")]
        public int Depth { get; set; }

        [Description("The number of bits in the stencil buffer.")]
        public int Stencil { get; set; }

        [Description("The number of samples to use for full screen anti-aliasing.")]
        public int Samples { get; set; }

        [Description("The color format of the accumulator buffer.")]
        public ColorFormatConfiguration AccumulatorFormat { get; set; }

        [Description("The number of render buffers. Typical values include one (single-), two (double-) or three (triple-buffering).")]
        public int Buffers { get; set; }

        [Description("Specifies whether to create a graphics mode with stereo capabilities.")]
        public bool Stereo { get; set; }

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
