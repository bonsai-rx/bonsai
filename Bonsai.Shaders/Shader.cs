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

        internal Shader(string name, ShaderWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            Name = name;
            shaderWindow = window;
        }

        [Obsolete]
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

        protected void CreateShaderState(
            IEnumerable<StateConfiguration> renderState,
            IEnumerable<UniformConfiguration> shaderUniforms,
            IEnumerable<BufferBindingConfiguration> bufferBindings,
            FramebufferConfiguration framebuffer)
        {
            program = CreateShader();
            GL.UseProgram(program);
            shaderState = new ShaderState(this, shaderWindow.ResourceManager, renderState, shaderUniforms, bufferBindings, framebuffer);
        }

        protected abstract int CreateShader();

        protected virtual Action OnDispatch()
        {
            return null;
        }

        public void Dispatch()
        {
            shaderState.Execute();
            var action = Interlocked.Exchange(ref update, null) + OnDispatch();
            if (action != null)
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
                    shaderState.Dispose();
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
