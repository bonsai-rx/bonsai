using Bonsai.Shaders.Configuration;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates the shader window with the specified
    /// display style and render settings.
    /// </summary>
    [DefaultProperty(nameof(ClearColor))]
    [TypeConverter(typeof(SettingsConverter))]
    [Description("Creates the shader window with the specified display style and render settings.")]
    [Editor("Bonsai.Shaders.Configuration.Design.ShaderScriptComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class CreateWindow : Source<ShaderWindow>
    {
        readonly ShaderWindowSettings configuration = new ShaderWindowSettings();

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateWindow"/> class.
        /// </summary>
        public CreateWindow()
        {
            Width = 640;
            Height = 480;
            VSync = VSyncMode.On;
            ClearColor = Color.Black;
            ClearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
            WindowState = WindowState.Normal;
            DisplayDevice = DisplayIndex.Default;
            GraphicsMode = new GraphicsModeConfiguration();
            TargetRenderFrequency = 60;
            CursorVisible = true;
            SwapSync = false;
        }

        /// <summary>
        /// Gets or sets the width of the shader window, in pixels.
        /// </summary>
        [Category("Window Style")]
        [Description("The width of the shader window, in pixels.")]
        public int Width
        {
            get { return configuration.Width; }
            set { configuration.Width = value; }
        }

        /// <summary>
        /// Gets or sets the height of the shader window, in pixels.
        /// </summary>
        [Category("Window Style")]
        [Description("The height of the shader window, in pixels.")]
        public int Height
        {
            get { return configuration.Height; }
            set { configuration.Height = value; }
        }

        /// <summary>
        /// Gets or sets the title of the shader window.
        /// </summary>
        [Category("Window Style")]
        [Description("The title of the shader window.")]
        public string Title
        {
            get { return configuration.Title; }
            set { configuration.Title = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the V-Sync configuration for shader
        /// window buffer swaps.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the V-Sync configuration for shader window buffer swaps.")]
        public VSyncMode VSync
        {
            get { return configuration.VSync; }
            set { configuration.VSync = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying whether to synchronize buffer swaps
        /// across application windows.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies whether to synchronize buffer swaps across application windows.")]
        public bool SwapSync
        {
            get { return configuration.SwapSync; }
            set { configuration.SwapSync = value; }
        }

        /// <summary>
        /// Gets or sets the color used to clear the framebuffer before rendering.
        /// </summary>
        [XmlIgnore]
        [Category("Render Settings")]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor
        {
            get { return configuration.ClearColor; }
            set { configuration.ClearColor = value; }
        }

        /// <summary>
        /// Gets or sets an HTML representation of the clear color value for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(ClearColor))]
        public string ClearColorHtml
        {
            get { return configuration.ClearColorHtml; }
            set { configuration.ClearColorHtml = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying which buffers to clear before rendering.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask
        {
            get { return configuration.ClearMask; }
            set { configuration.ClearMask = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying whether to hide or show the mouse cursor
        /// over the shader window.
        /// </summary>
        [Category("Window Style")]
        [Description("Specifies whether to hide or show the mouse cursor over the shader window.")]
        public bool CursorVisible
        {
            get { return configuration.CursorVisible; }
            set { configuration.CursorVisible = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the starting location of the shader window.
        /// If no value is specified, the window will be located at the center of the screen.
        /// </summary>
        [Category("Window Style")]
        [Description("Specifies the optional starting location of the shader window.")]
        public Point? Location
        {
            get { return configuration.Location; }
            set { configuration.Location = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the shader window border.
        /// </summary>
        [Category("Window Style")]
        [Description("Specifies the shader window border.")]
        public WindowBorder WindowBorder
        {
            get { return configuration.WindowBorder; }
            set { configuration.WindowBorder = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the starting state of the shader window.
        /// </summary>
        [Category("Window Style")]
        [Description("Specifies the starting state of the shader window.")]
        public WindowState WindowState
        {
            get { return configuration.WindowState; }
            set { configuration.WindowState = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the display device index on which to
        /// create the shader window.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the display device index on which to create the shader window.")]
        public DisplayIndex DisplayDevice
        {
            get { return configuration.DisplayDevice; }
            set { configuration.DisplayDevice = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the target render frequency. A value of zero
        /// indicates the maximum possible frequency will be used to generate render events.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the target render frequency.")]
        public double TargetRenderFrequency
        {
            get { return configuration.TargetRenderFrequency; }
            set { configuration.TargetRenderFrequency = value; }
        }

        /// <summary>
        /// Gets or sets a value specifying the target update frequency. If no value is
        /// specified, the value of the target render frequency will be used.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the target update frequency. If no value is specified, the target render frequency will be used.")]
        public double? TargetUpdateFrequency
        {
            get { return configuration.TargetUpdateFrequency; }
            set { configuration.TargetUpdateFrequency = value; }
        }

        /// <summary>
        /// Gets the collection of configuration objects specifying the initial render
        /// state of the shader window graphics context.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the initial render state of the shader window graphics context.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return configuration.RenderState; }
        }

        /// <summary>
        /// Gets or sets a value specifying the graphics mode of the shader window.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the graphics mode of the shader window.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public GraphicsModeConfiguration GraphicsMode
        {
            get { return configuration.GraphicsMode; }
            set { configuration.GraphicsMode = value; }
        }

        /// <summary>
        /// Generates an observable sequence that initializes and returns the
        /// shader window object. If a window has already been initialized, this
        /// source will return a reference to the created window.
        /// </summary>
        /// <returns>
        /// A sequence containing the <see cref="ShaderWindow"/> object.
        /// </returns>
        public override IObservable<ShaderWindow> Generate()
        {
            if (File.Exists(ShaderManager.DefaultConfigurationFile))
            {
                return Observable.Throw<ShaderWindow>(new InvalidOperationException(string.Format(
                    "{0} cannot be used together with a shader configuration file. " +
                    "If you want to control shader window creation, please delete the '{2}' file " +
                    "and specify window resources using the {1} operator.",
                    typeof(CreateWindow).Name,
                    typeof(Bonsai.Resources.LoadResources).Name,
                    ShaderManager.DefaultConfigurationFile)));
            }
            
            return ShaderManager.CreateWindow(configuration);
        }
    }
}
