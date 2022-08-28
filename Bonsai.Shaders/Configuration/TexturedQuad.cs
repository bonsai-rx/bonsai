using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for mesh resources using
    /// a simple textured quad geometry.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class TexturedQuad : MeshConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying quad geometry transformation effects.
        /// </summary>
        [Category("State")]
        [Description("Specifies quad geometry transformation effects.")]
        public QuadEffects QuadEffects { get; set; }

        /// <summary>
        /// Creates a new mesh resource using a textured quad geometry.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Mesh"/> class storing textured quad
        /// geometry.
        /// </returns>
        /// <inheritdoc/>
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
