using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class DisableState : StateConfiguration
    {
        public EnableCap Capability { get; set; }

        public override void Execute(Shader shader)
        {
            GL.Disable(Capability);
        }

        public override string ToString()
        {
            return string.Format("Disable({0})", Capability);
        }
    }
}
