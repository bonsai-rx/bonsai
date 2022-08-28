using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for initializing the value of a shader
    /// uniform variable with four floating-point components.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class Vec4Uniform : UniformConfiguration
    {
        /// <summary>
        /// Gets or sets the value used to initialize the shader uniform.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The value used to initialize the shader uniform.")]
        public Vector4 Value { get; set; }

        internal override void SetUniform(int location)
        {
            GL.Uniform4(location, Value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0}{1}", string.IsNullOrEmpty(Name) ? "Vec4" : Name, Value);
        }
    }
}
