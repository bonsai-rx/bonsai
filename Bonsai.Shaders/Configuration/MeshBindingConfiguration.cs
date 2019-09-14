using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(TypeName = "MeshBinding", Namespace = Constants.XmlNamespace)]
    public class MeshBindingConfiguration : BufferBindingConfiguration
    {
        [Description("The index of the binding point on which to bind the mesh buffer.")]
        public int Index { get; set; }

        [Category("Reference")]
        [TypeConverter(typeof(MeshNameConverter))]
        [Description("The name of the mesh buffer that will be bound to the shader.")]
        public string MeshName { get; set; }

        internal override BufferBinding CreateBufferBinding(Shader shader, ResourceManager resourceManager)
        {
            var mesh = resourceManager.Load<Mesh>(MeshName);
            return new MeshBinding(Index, mesh);
        }

        public override string ToString()
        {
            return ToString("BindBuffer", MeshName);
        }
    }
}
