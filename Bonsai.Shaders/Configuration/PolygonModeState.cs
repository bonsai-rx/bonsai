using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class PolygonModeState : StateConfiguration
    {
        public PolygonModeState()
        {
            Face = MaterialFace.FrontAndBack;
            Mode = PolygonMode.Fill;
        }

        [Description("Specifies the polygons that mode applies to.")]
        public MaterialFace Face { get; set; }

        [Description("Specifies how polygons will be rasterized.")]
        public PolygonMode Mode { get; set; }

        public override void Execute(ShaderWindow window)
        {
            GL.PolygonMode(Face, Mode);
        }

        public override string ToString()
        {
            return $"PolygonMode({Face}, {Mode})";
        }
    }
}
