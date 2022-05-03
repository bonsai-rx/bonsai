using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for specifying barriers to order
    /// memory operations.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class MemoryBarrierState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying which memory barriers to insert.
        /// </summary>
        [Description("Specifies which memory barriers to insert.")]
        public MemoryBarrierFlags Barriers { get; set; } = MemoryBarrierFlags.AllBarrierBits;

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.MemoryBarrier(Barriers);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"MemoryBarrier({Barriers})";
        }
    }
}
