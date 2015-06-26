using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class TexturedQuad : ShaderConfiguration
    {
        internal override void Configure(Shader shader)
        {
            base.Configure(shader);
            shader.Update(() =>
            {
                shader.DrawMode = PrimitiveType.Quads;
                shader.VertexCount = VertexHelper.TexturedQuad(
                    shader.VertexBuffer,
                    shader.VertexArray);
            });
        }
    }
}
