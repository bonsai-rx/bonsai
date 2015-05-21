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
        int texture;
        TexturedQuad quad;
        string vertexSource;
        string fragmentSource;
        event Action update;
        IGameWindow shaderWindow;
        Task shaderTask;

        internal Shader(string name, IGameWindow window, Task windowTask, string vertexShader, string fragmentShader)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            if (windowTask == null)
            {
                throw new ArgumentNullException("windowTask");
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
            shaderTask = windowTask;
            vertexSource = vertexShader;
            fragmentSource = fragmentShader;
            shaderWindow.UpdateFrame += Window_UpdateFrame;
            shaderWindow.RenderFrame += Window_RenderFrame;
        }

        internal bool Loaded { get; set; }

        public bool Visible { get; set; }

        public string Name { get; private set; }

        public int Program
        {
            get { return program; }
        }

        public int Texture
        {
            get { return texture; }
        }

        internal void Subscribe<TSource>(IObserver<TSource> observer)
        {
            shaderTask.GetAwaiter().OnCompleted(() =>
            {
                if (shaderTask.IsFaulted)
                {
                    observer.OnError(shaderTask.Exception.Flatten().InnerException);
                }
                else observer.OnCompleted();
            });
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

        private void Load()
        {
            texture = GL.GenTexture();
            quad = new TexturedQuad();
            program = CreateShader();
        }

        void Window_UpdateFrame(object sender, FrameEventArgs e)
        {
            if (!Loaded)
            {
                Load();
                Loaded = true;
            }
        }

        void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            if (Loaded && Visible)
            {
                GL.UseProgram(program);
                GL.BindTexture(TextureTarget.Texture2D, texture);
                var action = Interlocked.Exchange(ref update, null);
                if (action != null)
                {
                    action();
                }

                quad.Draw();
            }
        }

        public void Dispose()
        {
            if (Loaded)
            {
                Loaded = false;
                quad.Dispose();
                GL.DeleteTextures(1, ref texture);
                quad = null;
            }

            shaderWindow.RenderFrame -= Window_RenderFrame;
            shaderWindow.UpdateFrame -= Window_UpdateFrame;
        }
    }
}
