using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    abstract class BufferBinding
    {
        public abstract void Bind();

        public abstract void Unbind();
    }
}
