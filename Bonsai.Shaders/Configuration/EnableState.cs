using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for enabling the specified render
    /// state capability.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class EnableState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the render state capability to enable.
        /// </summary>
        [Description("Specifies the render state capability to enable.")]
        public EnableCap Capability { get; set; } = EnableCap.Blend;

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.Enable(Capability);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Enable({Capability})";
        }
    }
}
