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
        int timeLocation;
        string vertexSource;
        string geometrySource;
        string fragmentSource;
        event Action update;
        ShaderWindow shaderWindow;
        List<StateConfiguration> shaderState;
        List<TextureBindingConfiguration> shaderTextures;
        FramebufferConfiguration shaderFramebuffer;
        Mesh shaderMesh;
        double time;

        internal Shader(
            string name,
            ShaderWindow window,
            string vertexShader,
            string geometryShader,
            string fragmentShader,
            IEnumerable<StateConfiguration> renderState,
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

            if (renderState == null)
            {
                throw new ArgumentNullException("renderState");
            }

            if (textureBindings == null)
            {
                throw new ArgumentNullException("textureUnits");
            }

            if (framebuffer == null)
            {
                throw new ArgumentNullException("framebuffer");
            }

            Name = name;
            shaderWindow = window;
            vertexSource = vertexShader;
            geometrySource = geometryShader;
            fragmentSource = fragmentShader;
            shaderState = renderState.ToList();
            shaderTextures = textureBindings.ToList();
            shaderFramebuffer = framebuffer;
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

        public IEnumerable<TextureBindingConfiguration> TextureBindings
        {
            get { return shaderTextures; }
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
            time = 0;
            program = CreateShader();
            GL.UseProgram(program);
            foreach (var texture in shaderTextures)
            {
                texture.Load(this);
            }

            shaderFramebuffer.Load(this);
            timeLocation = GL.GetUniformLocation(program, "time");
        }

        public void Update(FrameEventArgs e)
        {
            if (Enabled)
            {
                time += e.Time;
                foreach (var state in shaderState)
                {
                    state.Execute(this);
                }

                GL.UseProgram(program);
                if (timeLocation >= 0)
                {
                    GL.Uniform1(timeLocation, (float)time);
                }

                var action = Interlocked.Exchange(ref update, null);
                foreach (var texture in shaderTextures)
                {
                    texture.Bind(this);
                }

                shaderFramebuffer.Bind(this);
                if (action != null)
                {
                    action();
                }

                var mesh = shaderMesh;
                if (mesh != null)
                {
                    mesh.Draw();
                }

                shaderFramebuffer.Unbind(this);
                foreach (var texture in shaderTextures)
                {
                    texture.Unbind(this);
                }
            }
        }

        public void Dispose()
        {
            if (shaderWindow != null)
            {
                shaderFramebuffer.Unload(this);
                foreach (var texture in shaderTextures)
                {
                    texture.Unload(this);
                }

                GL.DeleteProgram(program);
                shaderWindow = null;
                update = null;
            }
        }
    }
}
