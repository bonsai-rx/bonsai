using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object specifying the operation of blending
    /// for all draw buffers.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class BlendFunctionState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the scale factor for blending source
        /// color values.
        /// </summary>
        [Description("Specifies the scale factor for blending source color values.")]
        public BlendingFactor SourceFactor { get; set; } = BlendingFactor.SrcAlpha;

        /// <summary>
        /// Gets or sets a value specifying the scale factor for blending destination
        /// color values.
        /// </summary>
        [Description("Specifies the scale factor for blending destination color values.")]
        public BlendingFactor DestinationFactor { get; set; } = BlendingFactor.OneMinusSrcAlpha;

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            GL.BlendFunc(SourceFactor, DestinationFactor);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"BlendFunc({SourceFactor}, {DestinationFactor})";
        }
    }
}
