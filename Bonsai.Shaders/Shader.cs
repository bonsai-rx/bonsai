using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Shader(IGameWindow window, string vertexShader, string fragmentShader)
        {
            if (vertexShader == null)
            {
                throw new ArgumentNullException("vertexShader");
            }

            if (fragmentShader == null)
            {
                throw new ArgumentNullException("fragmentShader");
            }

            Window = window;
            vertexSource = vertexShader;
            fragmentSource = fragmentShader;
            Window.UpdateFrame += Window_UpdateFrame;
            Window.RenderFrame += Window_RenderFrame;
        }

        internal bool Loaded { get; set; }

        public IGameWindow Window { get; private set; }

        public bool Visible { get; set; }

        public int Program
        {
            get { return program; }
        }

        public int Texture
        {
            get { return texture; }
        }

        public void Update(Action action)
        {
            update += action;
        }

        static int CreateShader(string vertexSource, string fragmentSource)
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            GL.CompileShader(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            GL.CompileShader(fragmentShader);

            var shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            return shaderProgram;
        }

        private void Load()
        {
            texture = GL.GenTexture();
            quad = new TexturedQuad();
            program = CreateShader(vertexSource, fragmentSource);
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

            Window.RenderFrame -= Window_RenderFrame;
            Window.UpdateFrame -= Window_UpdateFrame;
        }
    }
}
