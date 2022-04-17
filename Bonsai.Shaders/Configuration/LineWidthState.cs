using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for specifying the width of rasterized
    /// lines.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class LineWidthState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the width of rasterized lines.
        /// </summary>
        [Description("Specifies the width of rasterized lines.")]
        public float Width { get; set; }

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.LineWidth(Width);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"LineWidth({Width})";
        }
    }
}
