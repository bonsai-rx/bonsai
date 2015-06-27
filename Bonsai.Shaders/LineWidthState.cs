using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class LineWidthState : StateConfiguration
    {
        public float Width { get; set; }

        public override void Execute()
        {
            GL.LineWidth(Width);
        }

        public override string ToString()
        {
            return string.Format("LineWidth({0})", Width);
        }
    }
}
