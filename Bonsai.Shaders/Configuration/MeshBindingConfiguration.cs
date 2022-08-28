using Bonsai.Resources;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for binding a mesh vertex buffer object
    /// to a shader uniform.
    /// </summary>
    [XmlType(TypeName = "MeshBinding", Namespace = Constants.XmlNamespace)]
    public class MeshBindingConfiguration : BufferBindingConfiguration
    {
        /// <summary>
        /// Gets or sets the index of the binding point on which to bind the
        /// mesh vertex buffer object.
        /// </summary>
        [Description("The index of the binding point on which to bind the mesh vertex buffer object.")]
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the mesh whose vertex buffer object will be
        /// bound to the shader uniform.
        /// </summary>
        [Category("Reference")]
        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh whose vertex buffer object will be bound to the shader uniform.")]
        public string MeshName { get; set; }

        internal override BufferBinding CreateBufferBinding(Shader shader, ResourceManager resourceManager)
        {
            var mesh = resourceManager.Load<Mesh>(MeshName);
            return new MeshBinding(Index, mesh);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString("BindBuffer", MeshName);
        }
    }
}
