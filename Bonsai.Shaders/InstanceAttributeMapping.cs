using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class InstanceAttributeMapping : VertexAttributeMapping
    {
        public InstanceAttributeMapping()
        {
            Divisor = 1;
        }

        public int Divisor { get; set; }

        public override string ToString()
        {
            return string.Format("InstanceAttribute({0}{1})", Type, Size);
        }
    }
}
