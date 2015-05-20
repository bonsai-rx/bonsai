using Chromatophore;
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
    public class Shader : GraphicsResource
    {
        int program;
        Texture2D texture;
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

        public Texture2D Texture
        {
            get { return texture; }
        }

        public void Update(Action action)
        {
            update += action;
        }

        private void Load()
        {
            quad = new TexturedQuad();
            texture = new Texture2D();
            program = ShaderPrograms.CreateShader(vertexSource, fragmentSource);
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
                GL.BindTexture(TextureTarget.Texture2D, texture.Name);
                var action = Interlocked.Exchange(ref update, null);
                if (action != null)
                {
                    action();
                }

                quad.Draw();
            }
        }

        protected override void ReleaseResource()
        {
            if (Loaded)
            {
                Loaded = false;
                texture.Dispose();
                quad.Dispose();
                texture = null;
                quad = null;
            }

            Window.RenderFrame -= Window_RenderFrame;
            Window.UpdateFrame -= Window_UpdateFrame;
            base.ReleaseResource();
        }
    }
}
