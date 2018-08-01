using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class ComputeProgram : Shader
    {
        string computeSource;

        internal ComputeProgram(
            string name,
            ShaderWindow window,
            string computeShader,
            IEnumerable<StateConfiguration> renderState,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<BufferBindingConfiguration> bufferBindings,
            FramebufferConfiguration framebuffer)
            : base(name, window)
        {
            if (computeShader == null)
            {
                throw new ArgumentNullException("computeShader", "No compute shader was specified for compute program " + name + ".");
            }

            computeSource = computeShader;
            CreateShaderState(renderState, shaderUniforms, bufferBindings, framebuffer);
        }

        public DispatchParameters WorkGroups { get; set; }

        protected override int CreateShader()
        {
            int status;
            var computeShader = GL.CreateShader(ShaderType.ComputeShader);
            try
            {
                GL.ShaderSource(computeShader, computeSource);
                GL.CompileShader(computeShader);
                GL.GetShader(computeShader, ShaderParameter.CompileStatus, out status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to compile compute shader.\nShader name: {0}\n{1}",
                        Name,
                        GL.GetShaderInfoLog(computeShader));
                    throw new ShaderException(message);
                }

                var shaderProgram = GL.CreateProgram();
                GL.AttachShader(shaderProgram, computeShader);
                GL.LinkProgram(shaderProgram);
                GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to link shader program.\nShader name: {0}\n{1}",
                        Name,
                        GL.GetProgramInfoLog(shaderProgram));
                    throw new ShaderException(message);
                }

                return shaderProgram;
            }
            finally { GL.DeleteShader(computeShader); }
        }

        protected override Action OnDispatch()
        {
            var workGroups = WorkGroups;
            if (workGroups.NumGroupsX == 0 || workGroups.NumGroupsY == 0 || workGroups.NumGroupsZ == 0)
            {
                return null;
            }

            return () => GL.DispatchCompute(workGroups.NumGroupsX, workGroups.NumGroupsY, workGroups.NumGroupsZ);
        }
    }
}
