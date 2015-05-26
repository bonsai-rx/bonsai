using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class ShaderWindow : GameWindow
    {
        List<Shader> shaders = new List<Shader>();
        static readonly object syncRoot = string.Intern("A1105A50-BBB0-4EC6-B8B2-B5EF38A9CC3E");

        public ShaderWindow(ShaderConfigurationCollection configuration)
        {
            foreach (var shaderConfiguration in configuration)
            {
                var shader = new Shader(
                    shaderConfiguration.Name, this,
                    shaderConfiguration.VertexShader,
                    shaderConfiguration.FragmentShader);
                shader.Enabled = shaderConfiguration.Enabled;
                shaders.Add(shader);
            }
        }

        public IEnumerable<Shader> Shaders
        {
            get { return shaders; }
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            foreach (var shader in shaders)
            {
                shader.Load();
            }

            base.OnLoad(e);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                if (WindowState == WindowState.Fullscreen)
                {
                    WindowState = WindowState.Normal;
                }
                else WindowState = WindowState.Fullscreen;
            }

            base.OnKeyDown(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (var shader in shaders)
            {
                shader.RenderFrame(e);
            }

            lock (syncRoot)
            {
                SwapBuffers();
            }
            base.OnRenderFrame(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            foreach (var shader in shaders)
            {
                shader.Dispose();
            }
            base.OnUnload(e);
        }
    }
}
