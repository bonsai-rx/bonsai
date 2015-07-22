using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class PointSprite : ShaderConfiguration
    {
        public PointSprite()
        {
            VertexShader = DefaultVertexShader;
            FragmentShader = DefaultFragmentShader;
        }

        const string DefaultVertexShader = @"
#version 400
uniform vec2 scale = vec2(1, 1);
uniform vec2 shift;
in vec2 vp;

void main()
{
  gl_Position = vec4(vp * scale + shift, 0.0, 1.0);
}
";

        const string DefaultFragmentShader = @"
#version 400
uniform sampler2D tex;
out vec4 frag_colour;

void main()
{
  vec4 texel = texture(tex, gl_PointCoord);
  frag_colour = texel;
}
";
    }
}
