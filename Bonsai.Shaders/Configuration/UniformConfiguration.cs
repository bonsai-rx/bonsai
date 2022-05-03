using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides an abstract base class for initializing the value of a shader
    /// uniform variable.
    /// </summary>
    [XmlInclude(typeof(FloatUniform))]
    [XmlInclude(typeof(Vec2Uniform))]
    [XmlInclude(typeof(Vec3Uniform))]
    [XmlInclude(typeof(Vec4Uniform))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class UniformConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the shader uniform to initialize.
        /// </summary>
        [Description("The name of the shader uniform to initialize.")]
        public string Name { get; set; }

        internal abstract void SetUniform(int location);
    }
}
