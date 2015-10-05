using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class ViewportState : StateConfiguration
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Width { get; set; }

        public float Height { get; set; }

        public override void Execute(Shader shader)
        {
            shader.Window.Viewport = new RectangleF(X, Y, Width, Height);
        }

        public override string ToString()
        {
            return string.Format("Viewport({0}, {1}, {2}, {3})", X, Y, Width, Height);
        }
    }
}
