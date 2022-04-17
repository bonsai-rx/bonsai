using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for specifying the polygon
    /// rasterization mode.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class PolygonModeState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the polygons that the rasterization
        /// mode applies to.
        /// </summary>
        [Description("Specifies the polygons that the rasterization mode applies to.")]
        public MaterialFace Face { get; set; } = MaterialFace.FrontAndBack;

        /// <summary>
        /// Gets or sets a value specifying how polygons will be rasterized.
        /// </summary>
        [Description("Specifies how polygons will be rasterized.")]
        public PolygonMode Mode { get; set; } = PolygonMode.Fill;

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.PolygonMode(Face, Mode);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"PolygonMode({Face}, {Mode})";
        }
    }
}
