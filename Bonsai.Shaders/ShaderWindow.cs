using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class ShaderWindow : GameWindow
    {
        RectangleF viewport;
        List<Shader> shaders = new List<Shader>();
        const string DefaultTitle = "Bonsai Shader Window";
        static readonly object syncRoot = string.Intern("A1105A50-BBB0-4EC6-B8B2-B5EF38A9CC3E");
        event Action update;

        public ShaderWindow(ShaderWindowSettings configuration)
            : base(configuration.Width, configuration.Height, GraphicsMode.Default,
                   DefaultTitle, GameWindowFlags.Default, DisplayDevice.GetDisplay(configuration.DisplayDevice))
        {
            VSync = configuration.VSync;
            Title = configuration.Title ?? DefaultTitle;
            WindowState = configuration.WindowState;
            Viewport = new RectangleF(0, 0, 1, 1);
            foreach (var shaderConfiguration in configuration.Shaders)
            {
                var shader = new Shader(
                    shaderConfiguration.Name, this,
                    shaderConfiguration.VertexShader,
                    shaderConfiguration.GeometryShader,
                    shaderConfiguration.FragmentShader,
                    shaderConfiguration.RenderState,
                    shaderConfiguration.TextureUnits);
                shaderConfiguration.Configure(shader);
                shaders.Add(shader);
            }
        }

        public RectangleF Viewport
        {
            get { return viewport; }
            set
            {
                viewport = value;
                UpdateViewport();
            }
        }

        public IEnumerable<Shader> Shaders
        {
            get { return shaders; }
        }

        internal void UpdateViewport()
        {
            UpdateViewport(Width, Height);
        }

        internal void UpdateViewport(float width, float height)
        {
            GL.Viewport(
                (int)(viewport.X * width),
                (int)(viewport.Y * height),
                (int)(viewport.Width * width),
                (int)(viewport.Height * height));
        }

        internal void Update(Action action)
        {
            update += action;
        }

        protected override void OnLoad(EventArgs e)
        {
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
            UpdateViewport();
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var action = Interlocked.Exchange(ref update, null);
            if (action != null)
            {
                action();
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            foreach (var shader in shaders)
            {
                shader.Update(e);
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
