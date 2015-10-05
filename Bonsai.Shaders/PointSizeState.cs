using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class PointSizeState : StateConfiguration
    {
        public float Size { get; set; }

        public override void Execute(Shader shader)
        {
            GL.PointSize(Size);
        }

        public override string ToString()
        {
            return string.Format("PointSize({0})", Size);
        }
    }
}
