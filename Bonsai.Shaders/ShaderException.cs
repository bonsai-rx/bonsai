using OpenTK;
using System;

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
