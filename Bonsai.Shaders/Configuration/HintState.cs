using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for specifying implementation-specific
    /// render hints.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class HintState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the implementation specific behavior
        /// to be controlled.
        /// </summary>
        [Description("Specifies the implementation specific behavior to be controlled.")]
        public HintTarget Target { get; set; } = HintTarget.PointSmoothHint;

        /// <summary>
        /// Gets or sets a value specifying the desired behavior.
        /// </summary>
        [Description("Specifies the desired behavior.")]
        public HintMode Mode { get; set; } = HintMode.Nicest;

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.Hint(Target, Mode);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Hint({Target}, {Mode})";
        }
    }
}
