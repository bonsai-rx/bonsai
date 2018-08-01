using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public abstract class Effect : Shader
    {
        string vertexSource;
        string geometrySource;
        string fragmentSource;

        internal Effect(
            string name,
            ShaderWindow window,
            string vertexShader,
            string geometryShader,
            string fragmentShader,
            IEnumerable<StateConfiguration> renderState,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<BufferBindingConfiguration> bufferBindings,
            FramebufferConfiguration framebuffer)
            : base(name, window)
        {
            if (vertexShader == null)
            {
                throw new ArgumentNullException("vertexShader", "No vertex shader was specified for material " + name + ".");
            }

            if (fragmentShader == null)
            {
                throw new ArgumentNullException("fragmentShader", "No fragment shader was specified for material " + name + ".");
            }

            vertexSource = vertexShader;
            geometrySource = geometryShader;
            fragmentSource = fragmentShader;
            CreateShaderState(renderState, shaderUniforms, bufferBindings, framebuffer);
        }

        protected override int CreateShader()
        {
            int status;
            int vertexShader = 0;
            int geometryShader = 0;
            int fragmentShader = 0;
            try
            {
                vertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(vertexShader, vertexSource);
                GL.CompileShader(vertexShader);
                GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to compile vertex shader.\nShader name: {0}\n{1}",
                        Name,
                        GL.GetShaderInfoLog(vertexShader));
                    throw new ShaderException(message);
                }

                geometryShader = 0;
                if (!string.IsNullOrWhiteSpace(geometrySource))
                {
                    geometryShader = GL.CreateShader(ShaderType.GeometryShader);
                    GL.ShaderSource(geometryShader, geometrySource);
                    GL.CompileShader(geometryShader);
                    GL.GetShader(geometryShader, ShaderParameter.CompileStatus, out status);
                    if (status == 0)
                    {
                        var message = string.Format(
                            "Failed to compile geometry shader.\nShader name: {0}\n{1}",
                            Name,
                            GL.GetShaderInfoLog(geometryShader));
                        throw new ShaderException(message);
                    }
                }

                fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(fragmentShader, fragmentSource);
                GL.CompileShader(fragmentShader);
                GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
                if (status == 0)
                {
                    var message = string.Format(
                        "Failed to compile fragment shader.\nShader name: {0}\n{1}",
                        Name,
                        GL.GetShaderInfoLog(fragmentShader));
                    throw new ShaderException(message);
                }

                var shaderProgram = GL.CreateProgram();
                GL.AttachShader(shaderProgram, vertexShader);
                if (geometryShader > 0) GL.AttachShader(shaderProgram, geometryShader);
                GL.AttachShader(shaderProgram, fragmentShader);
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
            finally
            {
                GL.DeleteShader(fragmentShader);
                GL.DeleteShader(geometryShader);
                GL.DeleteShader(vertexShader);
            }
        }
    }
}
