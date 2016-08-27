using Bonsai.Shaders.Configuration;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class Shader : IDisposable
    {
        int program;
        string vertexSource;
        string geometrySource;
        string fragmentSource;
        event Action update;
        ShaderWindow shaderWindow;
        ShaderState shaderState;
        Mesh shaderMesh;

        internal Shader(
            string name,
            ShaderWindow window,
            string vertexShader,
            string geometryShader,
            string fragmentShader,
            IEnumerable<StateConfiguration> renderState,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<TextureBindingConfiguration> textureBindings,
            FramebufferConfiguration framebuffer)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            if (vertexShader == null)
            {
                throw new ArgumentNullException("vertexShader");
            }

            if (fragmentShader == null)
            {
                throw new ArgumentNullException("fragmentShader");
            }

            Name = name;
            shaderWindow = window;
            vertexSource = vertexShader;
            geometrySource = geometryShader;
            fragmentSource = fragmentShader;
            shaderState = new ShaderState(this, renderState, shaderUniforms, textureBindings, framebuffer);
        }

        public bool Enabled { get; set; }

        public string Name { get; private set; }

        public Mesh Mesh
        {
            get { return shaderMesh; }
            internal set { shaderMesh = value; }
        }

        public int Program
        {
            get { return program; }
        }

        public ShaderWindow Window
        {
            get { return shaderWindow; }
        }

        public void Update(Action action)
        {
            update += action;
        }

        int CreateShader()
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

        public void Load()
        {
            program = CreateShader();
            GL.UseProgram(program);
            shaderState.Load();
        }

        public void Draw()
        {
            if (Enabled)
            {
                GL.UseProgram(program);

                var action = Interlocked.Exchange(ref update, null);
                shaderState.Bind();
                if (action != null)
                {
                    action();
                }

                var mesh = shaderMesh;
                if (mesh != null)
                {
                    mesh.Draw();
                }

                shaderState.Unbind();
            }
        }

        public void Dispose()
        {
            if (shaderWindow != null)
            {
                shaderState.Unload();
                GL.DeleteProgram(program);
                shaderWindow = null;
                update = null;
            }
        }
    }
}
