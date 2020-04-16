using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class TexturedQuad : MeshConfiguration
    {
        [Category("State")]
        [Description("Optional quad geometry transformation effects.")]
        public QuadEffects QuadEffects { get; set; }

        public override Mesh CreateResource(ResourceManager resourceManager)
        {
            var mesh = base.CreateResource(resourceManager);
            mesh.DrawMode = PrimitiveType.Quads;
            var flipX = (QuadEffects & QuadEffects.FlipHorizontally) != 0;
            var flipY = (QuadEffects & QuadEffects.FlipVertically) != 0;
            mesh.VertexCount = VertexHelper.TexturedQuad(
                mesh.VertexBuffer,
                mesh.VertexArray,
                flipX,
                flipY);
            return mesh;
        }
    }
}
