using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class DepthFunctionState : StateConfiguration
    {
        public DepthFunctionState()
        {
            Function = DepthFunction.Less;
        }

        public DepthFunction Function { get; set; }

        public override void Execute(Shader shader)
        {
            GL.DepthFunc(Function);
        }

        public override string ToString()
        {
            return string.Format("DepthFunc({0})", Function);
        }
    }
}
