using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    abstract class BufferBinding
    {
        public abstract void Load(Shader shader);

        public abstract void Bind(Shader shader);

        public abstract void Unbind(Shader shader);

        public abstract void Unload(Shader shader);
    }
}
