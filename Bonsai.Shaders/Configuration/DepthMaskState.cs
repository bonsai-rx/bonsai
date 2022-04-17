using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object specifying whether the depth buffer
    /// is enabled for writing.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class DepthMaskState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying whether the depth buffer is enabled
        /// for writing.
        /// </summary>
        [Description("Specifies whether the depth buffer is enabled for writing.")]
        public bool Enabled { get; set; } = true;

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.DepthMask(Enabled);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"DepthMask({Enabled})";
        }
    }
}
