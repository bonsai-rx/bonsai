using Bonsai.Resources;
using Bonsai.Shaders.Configuration;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Subjects;
using System.Threading;

namespace Bonsai.Shaders
{
    public class ShaderWindow : GameWindow
    {
        Color4 clearColor;
        ClearBufferMask clearMask;
        RectangleF viewport;
        RectangleF scissor;
        List<Shader> shaders;
        IDictionary<string, Texture> textures;
        IDictionary<string, Mesh> meshes;
        ResourceManager resourceManager;
        ShaderWindowSettings settings;
        const string DefaultTitle = "Bonsai Shader Window";
        static readonly RectangleF DefaultViewport = new RectangleF(0, 0, 1, 1);
        static readonly object syncRoot = string.Intern("A1105A50-BBB0-4EC6-B8B2-B5EF38A9CC3E");
        readonly Subject<FrameEvent> updateFrame;
        readonly Subject<FrameEvent> renderFrame;
        readonly bool swapSync;
        event Action update;

        public ShaderWindow(ShaderWindowSettings configuration)
            : this(configuration, DisplayDevice.GetDisplay(configuration.DisplayDevice) ?? DisplayDevice.Default)
        {
        }

        private ShaderWindow(ShaderWindowSettings configuration, DisplayDevice display)
            : base(configuration.Width, configuration.Height,
                   configuration.GraphicsMode == null ? GraphicsMode.Default : configuration.GraphicsMode.CreateGraphicsMode(),
                   DefaultTitle, GameWindowFlags.Default, display)
        {
            settings = configuration;
            VSync = configuration.VSync;
            swapSync = configuration.SwapSync;
            clearColor = configuration.ClearColor;
            clearMask = configuration.ClearMask;
            Title = configuration.Title ?? DefaultTitle;
            CursorVisible = configuration.CursorVisible;
            Location = configuration.Location.GetValueOrDefault(Location);
            WindowBorder = configuration.WindowBorder;
            WindowState = configuration.WindowState;
            Viewport = DefaultViewport;
            Scissor = DefaultViewport;
            TargetRenderFrequency = configuration.TargetRenderFrequency;
            TargetUpdateFrequency = configuration.TargetUpdateFrequency.GetValueOrDefault(
                VSync != VSyncMode.Off && configuration.TargetRenderFrequency == 0
                ? display.RefreshRate
                : configuration.TargetRenderFrequency);
            updateFrame = new Subject<FrameEvent>();
            renderFrame = new Subject<FrameEvent>();
            resourceManager = new ResourceManager();
            shaders = new List<Shader>();
        }

        internal IObservable<FrameEvent> UpdateFrameAsync
        {
            get { return updateFrame; }
        }

        internal IObservable<FrameEvent> RenderFrameAsync
        {
            get { return renderFrame; }
        }

        public Color ClearColor { get; set; }

        public RectangleF Viewport
        {
            get { return viewport; }
            set
            {
                viewport = value;
                UpdateViewport();
            }
        }

        public RectangleF Scissor
        {
            get { return scissor; }
            set
            {
                scissor = value;
                UpdateScissor();
            }
        }

        public IEnumerable<Shader> Shaders
        {
            get { return shaders; }
        }

        [Obsolete]
        public IDictionary<string, Texture> Textures
        {
            get
            {
                if (textures == null)
                {
                    textures = new ResourceDictionary<Texture>(resourceManager);
                }

                return textures;
            }
        }

        [Obsolete]
        public IDictionary<string, Mesh> Meshes
        {
            get
            {
                if (meshes == null)
                {
                    meshes = new ResourceDictionary<Mesh>(resourceManager);
                }

                return meshes;
            }
        }

        public ResourceManager ResourceManager
        {
            get { return resourceManager; }
        }

        internal void AddShader(Shader shader)
        {
            shaders.Add(shader);
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

        internal void UpdateScissor()
        {
            UpdateScissor(Width, Height);
        }

        internal void UpdateScissor(float width, float height)
        {
            GL.Scissor(
                (int)(scissor.X * width),
                (int)(scissor.Y * height),
                (int)(scissor.Width * width),
                (int)(scissor.Height * height));
        }

        public void Update(Action action)
        {
            update += action;
        }

        protected override void OnLoad(EventArgs e)
        {
            var windowManager = new WindowManagerConfiguration(this);
            var resources = new List<IResourceConfiguration>();
            resources.Add(windowManager);
            resources.AddRange(settings.Textures);
            resources.AddRange(settings.Meshes);
            resources.AddRange(settings.Shaders);
            resourceManager.Load(resources);
            foreach (var state in settings.RenderState)
            {
                state.Execute(this);
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
            UpdateScissor();
            base.OnResize(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            OnFrameEvent(updateFrame, TargetUpdatePeriod, e);
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (clearMask != ClearBufferMask.None)
            {
                if (viewport != DefaultViewport) Viewport = DefaultViewport;
                if (scissor != DefaultViewport) Scissor = DefaultViewport;
                GL.DepthMask(true);
                GL.ClearColor(clearColor);
                GL.Clear(clearMask);
            }

            var action = Interlocked.Exchange(ref update, null);
            if (action != null)
            {
                action();
            }

            OnFrameEvent(renderFrame, TargetRenderPeriod, e);
            base.OnRenderFrame(e);
            shaders.RemoveAll(shader =>
            {
                if (shader.Program != 0)
                {
                    shader.Dispatch();
                    return false;
                }
                return true;
            });

            if (!swapSync) SwapBuffers();
            else lock (syncRoot) SwapBuffers();
        }

        private void OnFrameEvent(Subject<FrameEvent> subject, double elapsedTime, FrameEventArgs e)
        {
            if (subject.HasObservers)
            {
                var frameEvent = new FrameEvent(this, elapsedTime, e);
                subject.OnNext(frameEvent);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            updateFrame.OnCompleted();
            renderFrame.OnCompleted();
            base.OnClosed(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            shaders.Clear();
            resourceManager.Dispose();
            base.OnUnload(e);
        }

        class WindowManagerConfiguration : ResourceConfiguration<WindowManager>
        {
            readonly ShaderWindow window;

            internal WindowManagerConfiguration(ShaderWindow owner)
            {
                Name = string.Empty;
                window = owner;
            }

            public override WindowManager CreateResource(ResourceManager resourceManager)
            {
                return new WindowManager(window);
            }
        }
    }

    class WindowManager : IDisposable
    {
        internal WindowManager(ShaderWindow window)
        {
            Window = window;
        }

        internal ShaderWindow Window { get; private set; }

        public void Dispose()
        {
        }
    }
}
