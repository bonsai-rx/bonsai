using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        [Description("Specifies the number of instances populated by the buffer attribute.")]
        public int Divisor { get; set; }

        public override string ToString()
        {
            var size = Size;
            return string.Format("InstanceAttribute({0}{1})", Type, size > 1 ? (object)size : null);
        }
    }
}
