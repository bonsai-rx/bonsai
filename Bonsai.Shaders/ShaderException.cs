using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Serializable]
    public class ShaderException : GraphicsException
    {
        public ShaderException()
        {
        }

        public ShaderException(string message)
            : base(message)
        {
        }
    }
}
