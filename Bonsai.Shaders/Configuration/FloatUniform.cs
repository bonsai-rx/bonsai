using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for initializing the value of a shader
    /// uniform variable with one floating-point component.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class FloatUniform : UniformConfiguration
    {
        /// <summary>
        /// Gets or sets the value used to initialize the shader uniform.
        /// </summary>
        [Description("The value used to initialize the shader uniform.")]
        public float Value { get; set; }

        internal override void SetUniform(int location)
        {
            GL.Uniform1(location, Value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{(string.IsNullOrEmpty(Name) ? "Float" : Name)}({Value})";
        }
    }
}
