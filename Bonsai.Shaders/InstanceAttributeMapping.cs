using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents the format of a per-instance defined input value to a
    /// vertex shader.
    /// </summary>
    public class InstanceAttributeMapping : VertexAttributeMapping
    {
        /// <summary>
        /// Gets or sets a value specifying the number of instances that each attribute
        /// in the buffer represents during instanced rendering.
        /// </summary>
        /// <remarks>
        /// If divisor is zero, the attribute advances once per vertex. If divisor
        /// is non-zero, the attribute advances once per divisor instances of the
        /// sets of vertices being rendered.
        /// </remarks>
        [Description("Specifies the number of instances that each attribute in the buffer represents during instanced rendering.")]
        public int Divisor { get; set; } = 1;

        /// <inheritdoc/>
        public override string ToString()
        {
            var size = Size;
            return string.Format("InstanceAttribute({0}{1})", Type, size > 1 ? (object)size : null);
        }
    }
}
