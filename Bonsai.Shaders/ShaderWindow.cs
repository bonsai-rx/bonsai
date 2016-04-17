using Bonsai.Shaders.Configuration;
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
        List<Shader> shaders;
        Dictionary<string, Texture> textures;
        Dictionary<string, Mesh> meshes;
        ShaderWindowSettings settings;
        const string DefaultTitle = "Bonsai Shader Window";
        static readonly object syncRoot = string.Intern("A1105A50-BBB0-4EC6-B8B2-B5EF38A9CC3E");
        event Action update;

        public ShaderWindow(ShaderWindowSettings configuration)
            : base(configuration.Width, configuration.Height, GraphicsMode.Default,
                   DefaultTitle, GameWindowFlags.Default, DisplayDevice.GetDisplay(configuration.DisplayDevice))
        {
            settings = configuration;
            VSync = configuration.VSync;
            Title = configuration.Title ?? DefaultTitle;
            WindowState = configuration.WindowState;
            Viewport = new RectangleF(0, 0, 1, 1);
            TargetRenderFrequency = configuration.TargetRenderFrequency;
            TargetUpdateFrequency = configuration.TargetRenderFrequency;
            textures = new Dictionary<string, Texture>();
            meshes = new Dictionary<string, Mesh>();
            shaders = settings.Shaders
                .Select(shaderConfiguration => shaderConfiguration.CreateShader(this))
                .ToList();
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

        public Dictionary<string, Texture> Textures
        {
            get { return textures; }
        }

        public Dictionary<string, Mesh> Meshes
        {
            get { return meshes; }
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
            foreach (var configuration in settings.Textures)
            {
                textures.Add(configuration.Name, configuration.CreateResource());
            }

            foreach (var configuration in settings.Meshes)
            {
                meshes.Add(configuration.Name, configuration.CreateResource());
            }

            foreach (var shader in shaders)
            {
                var configuration = settings.Shaders[shader.Name];
                if (!string.IsNullOrEmpty(configuration.MeshName))
                {
                    shader.Mesh = meshes[configuration.MeshName];
                }

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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            var action = Interlocked.Exchange(ref update, null);
            if (action != null)
            {
                action();
            }

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

            foreach (var texture in textures.Values)
            {
                texture.Dispose();
            }

            foreach (var resource in meshes.Values)
            {
                resource.Dispose();
            }
            base.OnUnload(e);
        }
    }
}
