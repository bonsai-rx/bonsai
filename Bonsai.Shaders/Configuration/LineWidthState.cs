using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class LineWidthState : StateConfiguration
    {
        [Description("Specifies the width of rasterized lines.")]
        public float Width { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.LineWidth(Width);
        }

        public override string ToString()
        {
            return string.Format("LineWidth({0})", Width);
        }
    }
}
