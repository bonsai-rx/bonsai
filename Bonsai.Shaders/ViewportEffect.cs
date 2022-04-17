using Bonsai.Shaders.Configuration;
using System;
using System.Collections.Generic;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Provides functionality for rendering a fullscreen effect using a fragment
    /// shader pass.
    /// </summary>
    public class ViewportEffect : Effect
    {
        Mesh mesh;
        const string VertexShader = @"
#version 400
in vec2 vp;
in vec2 vt;
out vec2 texCoord;

void main()
{
  gl_Position = vec4(vp, 0.0, 1.0);
  texCoord = vt;
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
            Mesh texturedQuad)
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
            mesh = texturedQuad ?? throw new ArgumentNullException(nameof(texturedQuad));
        }

        /// <inheritdoc/>
        protected override Action OnDispatch()
        {
            return mesh.Draw;
        }

        /// <inheritdoc/>
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
