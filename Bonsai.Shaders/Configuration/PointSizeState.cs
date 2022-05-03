using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for specifying the diameter of
    /// rasterized points.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class PointSizeState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the diameter of rasterized points.
        /// </summary>
        [Description("Specifies the diameter of rasterized points.")]
        public float Size { get; set; }

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.PointSize(Size);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"PointSize({Size})";
        }
    }
}
