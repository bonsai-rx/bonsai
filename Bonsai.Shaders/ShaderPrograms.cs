using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    static class ShaderPrograms
    {
        public const string UniformScaleShiftTexCoord = @"
#version 400
uniform vec2 scale = vec2(1, 1);
uniform vec2 shift;
in vec2 vp;
in vec2 vt;
out vec2 tex_coord;
void main()
{
  gl_Position = vec4(vp * scale + shift, 0.0, 1.0);
  tex_coord = vt;
}
";

        public const string UniformSampler = @"
#version 400
uniform sampler2D tex;
in vec2 tex_coord;
out vec4 frag_colour;
void main()
{
  vec4 texel = texture(tex, tex_coord);
  frag_colour = texel;
}
";
    }
}
