using Bonsai.Shaders.Configuration;
using System;
using System.Collections.Generic;

namespace Bonsai.Shaders
{
    public class Material : Effect
    {
        Mesh materialMesh;

        internal Material(
            string name,
            ShaderWindow window,
            string vertexShader,
            string geometryShader,
            string fragmentShader,
            IEnumerable<StateConfiguration> renderState,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<BufferBindingConfiguration> bufferBindings,
            FramebufferConfiguration framebuffer)
            : base(name,
                   window,
                   vertexShader,
                   geometryShader,
                   fragmentShader,
                   renderState,
                   shaderUniforms,
                   bufferBindings,
                   framebuffer)
        {
        }

        public Mesh Mesh
        {
            get { return materialMesh; }
            internal set { materialMesh = value; }
        }

        protected override Action OnDispatch()
        {
            var mesh = materialMesh;
            return mesh != null ? mesh.Draw : (Action)null;
        }
    }
}
