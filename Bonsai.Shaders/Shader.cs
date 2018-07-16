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
    public abstract class Shader : IDisposable
    {
        int program;
        event Action update;
        ShaderWindow shaderWindow;
        ShaderState shaderState;

        internal Shader(
            string name,
            ShaderWindow window,
            IEnumerable<StateConfiguration> renderState,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<BufferBindingConfiguration> bufferBindings,
            FramebufferConfiguration framebuffer)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            Name = name;
            shaderWindow = window;
            shaderState = new ShaderState(this, renderState, shaderUniforms, bufferBindings, framebuffer);
        }

        public bool Enabled { get; set; }

        public string Name { get; private set; }

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

        protected abstract int CreateShader();

        protected virtual void OnLoad()
        {
        }

        public void Load()
        {
            program = CreateShader();
            GL.UseProgram(program);
            shaderState.Load();
            OnLoad();
        }

        protected virtual Action OnDispatch()
        {
            return null;
        }

        public void Dispatch()
        {
            var action = Interlocked.Exchange(ref update, null) + OnDispatch();
            if (action != null && Enabled)
            {
                GL.UseProgram(program);
                shaderState.Bind();
                action();
                shaderState.Unbind();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (program != 0)
            {
                if (disposing)
                {
                    shaderState.Unload();
                    GL.DeleteProgram(program);
                    shaderWindow = null;
                    update = null;
                    program = 0;
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
    }
}
