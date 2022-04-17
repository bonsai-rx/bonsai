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
    /// <summary>
    /// Creates and renders a window using the specified resources and a programmable
    /// shader pipeline.
    /// </summary>
    public class ShaderWindow : GameWindow
    {
        RectangleF viewport;
        RectangleF scissor;
        readonly List<Shader> shaders;
        readonly ClearBufferMask clearMask;
        readonly ResourceManager resourceManager;
        readonly ShaderWindowSettings settings;
        const string DefaultTitle = "Bonsai Shader Window";
        static readonly RectangleF DefaultViewport = new RectangleF(0, 0, 1, 1);
        static readonly object syncRoot = string.Intern("A1105A50-BBB0-4EC6-B8B2-B5EF38A9CC3E");
        readonly Subject<FrameEvent> updateFrame;
        readonly Subject<FrameEvent> renderFrame;
        readonly bool swapSync;
        event Action update;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderWindow"/> class using
        /// the specified window configuration settings.
        /// </summary>
        /// <param name="configuration">
        /// The configuration settings used to initialize the shader window.
        /// </param>
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
            ClearColor = configuration.ClearColor;
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

        /// <summary>
        /// Gets or sets the color used to clear the framebuffer before rendering.
        /// </summary>
        public Color ClearColor { get; set; }

        /// <summary>
        /// Gets or sets the active viewport for rendering, in normalized coordinates.
        /// </summary>
        public RectangleF Viewport
        {
            get { return viewport; }
            set
            {
                viewport = value;
                UpdateViewport();
            }
        }

        /// <summary>
        /// Gets or sets the active scissor box, in normalized coordinates. Any fragments
        /// falling outside the scissor box will be discarded.
        /// </summary>
        public RectangleF Scissor
        {
            get { return scissor; }
            set
            {
                scissor = value;
                UpdateScissor();
            }
        }

        /// <summary>
        /// Gets the collection of shaders specifying the active render pipeline.
        /// </summary>
        public IEnumerable<Shader> Shaders
        {
            get { return shaders; }
        }

        /// <summary>
        /// Gets the resource manager used to load and release sets of render
        /// resources to the shader window.
        /// </summary>
        public ResourceManager ResourceManager
        {
            get { return resourceManager; }
        }

        /// <summary>
        /// Gets or sets the size of the OpenGL surface in window coordinates.
        /// The coordinates are specified in device-dependent pixels.
        /// </summary>
        public new Size ClientSize
        {
            get { return WindowBorder == WindowBorder.Hidden ? Size : base.ClientSize; }
            set
            {
                if (WindowBorder == WindowBorder.Hidden)
                {
                    Size = value;
                }
                else base.ClientSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the width of the OpenGL surface in window coordinates.
        /// The coordinates are specified in device-dependent pixels.
        /// </summary>
        public new int Width
        {
            get { return WindowBorder == WindowBorder.Hidden ? Size.Width : base.Width; }
            set
            {
                if (WindowBorder == WindowBorder.Hidden)
                {
                    Size = new Size(value, Size.Height);
                }
                else base.Width = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the OpenGL surface in window coordinates.
        /// The coordinates are specified in device-dependent pixels.
        /// </summary>
        public new int Height
        {
            get { return WindowBorder == WindowBorder.Hidden ? Size.Height : base.Height; }
            set
            {
                if (WindowBorder == WindowBorder.Hidden)
                {
                    Size = new Size(Size.Width, value);
                }
                else base.Height = value;
            }
        }

        internal void AddShader(Shader shader)
        {
            shaders.Add(shader);
        }

        internal void UpdateViewport()
        {
            var size = ClientSize;
            UpdateViewport(size.Width, size.Height);
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
            var size = ClientSize;
            UpdateScissor(size.Width, size.Height);
        }

        internal void UpdateScissor(float width, float height)
        {
            GL.Scissor(
                (int)(scissor.X * width),
                (int)(scissor.Y * height),
                (int)(scissor.Width * width),
                (int)(scissor.Height * height));
        }

        /// <summary>
        /// Queues a render command or state update.
        /// </summary>
        /// <param name="action">
        /// The action that will execute when the next frame is rendered.
        /// </param>
        public void Update(Action action)
        {
            update += action;
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            var windowManager = new WindowManagerConfiguration(this);
            var resources = new List<IResourceConfiguration>();
            resources.Add(windowManager);
#pragma warning disable CS0612 // Type or member is obsolete
            resources.AddRange(settings.Textures);
            resources.AddRange(settings.Meshes);
            resources.AddRange(settings.Shaders);
#pragma warning restore CS0612 // Type or member is obsolete
            resourceManager.Load(resources);
            foreach (var state in settings.RenderState)
            {
                state.Execute(this);
            }
            base.OnLoad(e);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void OnResize(EventArgs e)
        {
            UpdateViewport();
            UpdateScissor();
            base.OnResize(e);
        }

        /// <inheritdoc/>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            OnFrameEvent(updateFrame, TargetUpdatePeriod, e);
            base.OnUpdateFrame(e);
        }

        /// <inheritdoc/>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (clearMask != ClearBufferMask.None)
            {
                if (viewport != DefaultViewport) Viewport = DefaultViewport;
                if (scissor != DefaultViewport) Scissor = DefaultViewport;
                GL.DepthMask(true);
                GL.ClearColor(ClearColor);
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

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            updateFrame.OnCompleted();
            renderFrame.OnCompleted();
            base.OnClosed(e);
        }

        /// <inheritdoc/>
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
