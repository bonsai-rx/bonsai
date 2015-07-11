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
        int vbo;
        int vao;
        int eao;
        int fbo;
        int program;
        int timeLocation;
        string vertexSource;
        string geometrySource;
        string fragmentSource;
        event Action update;
        ShaderWindow shaderWindow;
        List<StateConfiguration> shaderState;
        List<TextureConfiguration> shaderTextures;
        double time;

        internal Shader(
            string name,
            ShaderWindow window,
            string vertexShader,
            string geometryShader,
            string fragmentShader,
            IEnumerable<StateConfiguration> renderState,
            IEnumerable<TextureConfiguration> textureUnits)
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

            if (textureUnits == null)
            {
                throw new ArgumentNullException("textureUnits");
            }

            Name = name;
            shaderWindow = window;
            vertexSource = vertexShader;
            geometrySource = geometryShader;
            fragmentSource = fragmentShader;
            shaderState = renderState.ToList();
            shaderTextures = textureUnits.ToList();
        }

        public bool Enabled { get; set; }

        public string Name { get; private set; }

        public PrimitiveType DrawMode { get; set; }

        public int VertexCount { get; set; }

        public int VertexBuffer
        {
            get { return vbo; }
        }

        public int VertexArray
        {
            get { return vao; }
        }

        public int ElementArray
        {
            get { return eao; }
        }

        public int Framebuffer
        {
            get { return fbo; }
        }

        public int Program
        {
            get { return program; }
        }

        public ShaderWindow Window
        {
            get { return shaderWindow; }
        }

        public IEnumerable<TextureConfiguration> TextureUnits
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
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
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

            var geometryShader = 0;
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

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
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

        internal void EnsureFramebuffer()
        {
            if (fbo == 0)
            {
                GL.GenFramebuffers(1, out fbo);
            }
        }

        internal void EnsureElementArray()
        {
            if (eao == 0)
            {
                GL.GenBuffers(1, out eao);
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

            GL.GenBuffers(1, out vbo);
            GL.GenVertexArrays(1, out vao);
            timeLocation = GL.GetUniformLocation(program, "time");
        }

        public void RenderFrame(FrameEventArgs e)
        {
            time += e.Time;
            if (Enabled)
            {
                foreach (var state in shaderState)
                {
                    state.Execute();
                }

                GL.UseProgram(program);
                if (timeLocation >= 0)
                {
                    GL.Uniform1(timeLocation, (float)time);
                }

                var action = Interlocked.Exchange(ref update, null);
                if (action != null)
                {
                    action();
                }

                if (VertexCount > 0)
                {
                    foreach (var texture in shaderTextures)
                    {
                        texture.Bind(this);
                    }

                    GL.BindVertexArray(vao);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                    if (fbo > 0) GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                    if (eao > 0)
                    {
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, eao);
                        GL.DrawElements(DrawMode, VertexCount, DrawElementsType.UnsignedShort, IntPtr.Zero);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                    }
                    else GL.DrawArrays(DrawMode, 0, VertexCount);
                    if (fbo > 0) GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    GL.BindVertexArray(0);

                    foreach (var texture in shaderTextures)
                    {
                        texture.Unbind(this);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (shaderWindow != null)
            {
                foreach (var texture in shaderTextures)
                {
                    texture.Unload(this);
                }

                if (fbo != 0) GL.DeleteFramebuffers(1, ref fbo);
                if (eao != 0) GL.DeleteBuffers(1, ref eao);
                GL.DeleteVertexArrays(1, ref vao);
                GL.DeleteBuffers(1, ref vbo);
                GL.DeleteProgram(program);
                shaderWindow = null;
                update = null;
            }
        }
    }
}
