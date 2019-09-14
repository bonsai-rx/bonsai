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
    [XmlInclude(typeof(TexturedQuad))]
    [XmlInclude(typeof(TexturedModel))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class MeshConfiguration : ResourceConfiguration<Mesh>
    {
        public override Mesh CreateResource(ResourceManager resourceManager)
        {
            return new Mesh();
        }
    }
}
