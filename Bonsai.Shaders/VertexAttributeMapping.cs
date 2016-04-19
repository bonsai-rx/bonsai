using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class VertexAttributeMapping
    {
        public VertexAttributeMapping()
        {
            Size = 4;
            Type = VertexAttribPointerType.Float;
        }

        [Description("Specifies the number of components in the vertex attribute.")]
        public int Size { get; set; }

        [Description("Specifies whether fixed-point data values should be normalized or converted directly before they are accessed.")]
        public bool Normalized { get; set; }

        [Description("Specifies the data type of each component in the vertex attribute.")]
        public VertexAttribPointerType Type { get; set; }

        public override string ToString()
        {
            var size = Size;
            return string.Format("VertexAttribute({0}{1})", Type, size > 1 ? (object)size : null);
        }
    }
}
