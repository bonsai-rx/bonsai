using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for disabling the specified render
    /// state capability.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class DisableState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the render state capability to disable.
        /// </summary>
        [Description("Specifies the render state capability to disable.")]
        public EnableCap Capability { get; set; } = EnableCap.Blend;

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.Disable(Capability);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Disable({Capability})";
        }
    }
}
