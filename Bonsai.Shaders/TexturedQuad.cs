using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class TexturedQuad : ShaderConfiguration
    {
        public TexturedQuad()
        {
            VertexShader = DefaultVertexShader;
            FragmentShader = DefaultFragmentShader;
        }

        [Category("State")]
        public QuadEffects QuadEffects { get; set; }

        internal override void Configure(Shader shader)
        {
            base.Configure(shader);
            shader.Update(() =>
            {
                var flipX = (QuadEffects & QuadEffects.FlipHorizontally) != 0;
                var flipY = (QuadEffects & QuadEffects.FlipVertically) != 0;
                shader.DrawMode = PrimitiveType.Quads;
                shader.VertexCount = VertexHelper.TexturedQuad(
                    shader.VertexBuffer,
                    shader.VertexArray,
                    flipX,
                    flipY);
            });
        }

        const string DefaultVertexShader = @"
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

        const string DefaultFragmentShader = @"
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
