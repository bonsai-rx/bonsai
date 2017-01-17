using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class ViewportEffect : Effect
    {
        Mesh mesh;
        TexturedQuad quad;
        const string VertexShader = @"
in vec2 vp;
in vec2 vt;
out vec2 tex_coord;

void main()
{
  gl_Position = vec4(vp, 0.0, 1.0);
  tex_coord = vt;
}
";

        internal ViewportEffect(
            string name,
            ShaderWindow window,
            string fragmentShader,
            IEnumerable<StateConfiguration> renderState,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<BufferBindingConfiguration> bufferBindings,
            FramebufferConfiguration framebuffer,
            TexturedQuad texturedQuad)
            : base(name,
                   window,
                   VertexShader,
                   null,
                   fragmentShader,
                   renderState,
                   shaderUniforms,
                   bufferBindings,
                   framebuffer)
        {
            if (texturedQuad == null)
            {
                throw new ArgumentNullException("texturedQuad");
            }

            quad = texturedQuad;
        }

        protected override void OnLoad()
        {
            mesh = quad.CreateResource();
            base.OnLoad();
        }

        protected override void OnDispatch()
        {
            mesh.Draw();
        }

        protected override void Dispose(bool disposing)
        {
            if (mesh != null)
            {
                if (disposing)
                {
                    mesh.Dispose();
                    mesh = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
