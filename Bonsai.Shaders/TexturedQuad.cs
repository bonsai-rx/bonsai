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
    }
}
