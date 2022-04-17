using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object specifying the function used for depth
    /// buffer comparisons.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class DepthFunctionState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the function used for depth buffer
        /// comparisons.
        /// </summary>
        [Description("Specifies the function used for depth buffer comparisons.")]
        public DepthFunction Function { get; set; } = DepthFunction.Less;

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.DepthFunc(Function);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"DepthFunc({Function})";
        }
    }
}
