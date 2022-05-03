using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Provides common functionality for executing and updating the state of
    /// compiled shader programs.
    /// </summary>
    public abstract class Shader : IDisposable
    {
        int program;
        event Action update;
        ShaderWindow shaderWindow;
        ShaderState shaderState;

        internal Shader(string name, ShaderWindow window)
        {
            Name = name;
            shaderWindow = window ?? throw new ArgumentNullException(nameof(window));
        }

        /// <summary>
        /// Gets the name of the shader.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the handle to the shader program object.
        /// </summary>
        public int Program
        {
            get { return program; }
        }

        /// <summary>
        /// Gets the window containing the graphics context on which to render
        /// the shader program.
        /// </summary>
        public ShaderWindow Window
        {
            get { return shaderWindow; }
        }

        /// <summary>
        /// Schedules an action for execution when running the shader program.
        /// Any render operations called as part of the action will execute
        /// in the context of this shader program.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action"/> to invoke when running the shader program.
        /// </param>
        public void Update(Action action)
        {
            update += action;
        }

        /// <summary>
        /// Initializes the shader state object used to specify the render state,
        /// uniform values, buffer bindings and framebuffer configuration to use when
        /// running the shader program.
        /// </summary>
        /// <param name="renderState">
        /// The collection of configuration objects specifying the render states required
        /// for running the shader program.
        /// </param>
        /// <param name="shaderUniforms">
        /// The collection of configuration objects specifying the default values of
        /// uniform variables in the shader program.
        /// </param>
        /// <param name="bufferBindings">
        /// The collection of configuration objects specifying the buffer bindings
        /// to set before running the shader.
        /// </param>
        /// <param name="framebuffer">
        /// The configuration state of the framebuffer object used for render to
        /// texture passes.
        /// </param>
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

        /// <summary>
        /// When overridden in a derived class, compiles the shader program and
        /// returns the program object handle.
        /// </summary>
        /// <returns>
        /// A handle to the shader program object.
        /// </returns>
        protected abstract int CreateShader();

        /// <summary>
        /// Returns any actions that should be executed before running the shader program.
        /// </summary>
        /// <returns>
        /// An <see cref="Action"/> object to be called before running the shader program.
        /// If the return value is <see langword="null"/>, no action will be executed.
        /// </returns>
        protected virtual Action OnDispatch()
        {
            return null;
        }

        /// <summary>
        /// Loads the shader program into the current render state and dispatches
        /// all pending render operations.
        /// </summary>
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

        /// <summary>
        /// Releases all resources used by the <see cref="Shader"/> class.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
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

        /// <summary>
        /// Releases all resources used by the <see cref="Shader"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
