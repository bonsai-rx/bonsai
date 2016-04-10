using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class VertexAttributeMapping
    {
        public int Size { get; set; }

        public bool Normalized { get; set; }

        public VertexAttribPointerType Type { get; set; }

        public override string ToString()
        {
            return string.Format("VertexAttribute({0}{1})", Type, Size);
        }
    }
}
