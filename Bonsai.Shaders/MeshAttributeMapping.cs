using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    struct MeshAttributeMapping
    {
        public Mesh Mesh;
        public int Divisor;

        public MeshAttributeMapping(Mesh mesh, int divisor)
        {
            Mesh = mesh;
            Divisor = divisor;
        }
    }
}
