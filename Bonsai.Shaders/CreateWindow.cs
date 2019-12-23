using Bonsai.Shaders.Configuration;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [DefaultProperty("ClearColor")]
    [TypeConverter(typeof(SettingsConverter))]
    [Description("Creates the shader window with the specified display style and render settings.")]
    [Editor("Bonsai.Shaders.Configuration.Design.ShaderScriptComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class CreateWindow : Source<ShaderWindow>
    {
        readonly ShaderWindowSettings configuration = new ShaderWindowSettings();

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
            CursorVisible = true;
            SwapSync = false;
        }

        [Category("Window Style")]
        [Description("The width of the shader window surface.")]
        public int Width
        {
            get { return configuration.Width; }
            set { configuration.Width = value; }
        }

        [Category("Window Style")]
        [Description("The height of the shader window surface.")]
        public int Height
        {
            get { return configuration.Height; }
            set { configuration.Height = value; }
        }

        [Category("Window Style")]
        [Description("The title of the shader window.")]
        public string Title
        {
            get { return configuration.Title; }
            set { configuration.Title = value; }
        }

        [Category("Render Settings")]
        [Description("Specifies V-Sync configuration for the shader window.")]
        public VSyncMode VSync
        {
            get { return configuration.VSync; }
            set { configuration.VSync = value; }
        }

        [Category("Render Settings")]
        [Description("Specifies whether to synchronize buffer swaps across application windows.")]
        public bool SwapSync
        {
            get { return configuration.SwapSync; }
            set { configuration.SwapSync = value; }
        }

        [XmlIgnore]
        [Category("Render Settings")]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor
        {
            get { return configuration.ClearColor; }
            set { configuration.ClearColor = value; }
        }

        [Browsable(false)]
        [XmlElement("ClearColor")]
        public string ClearColorHtml
        {
            get { return configuration.ClearColorHtml; }
            set { configuration.ClearColorHtml = value; }
        }

        [Category("Render Settings")]
        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask
        {
            get { return configuration.ClearMask; }
            set { configuration.ClearMask = value; }
        }

        [Category("Window Style")]
        [Description("Specifies whether to hide or show the mouse cursor over the shader window.")]
        public bool CursorVisible
        {
            get { return configuration.CursorVisible; }
            set { configuration.CursorVisible = value; }
        }

        [Category("Window Style")]
        [Description("The optional starting location of the shader window.")]
        public Point? Location
        {
            get { return configuration.Location; }
            set { configuration.Location = value; }
        }

        [Category("Window Style")]
        [Description("The initial shader window border.")]
        public WindowBorder WindowBorder
        {
            get { return configuration.WindowBorder; }
            set { configuration.WindowBorder = value; }
        }

        [Category("Window Style")]
        [Description("The initial shader window state.")]
        public WindowState WindowState
        {
            get { return configuration.WindowState; }
            set { configuration.WindowState = value; }
        }

        [Category("Render Settings")]
        [Description("The display device index on which to create the shader window.")]
        public DisplayIndex DisplayDevice
        {
            get { return configuration.DisplayDevice; }
            set { configuration.DisplayDevice = value; }
        }

        [Category("Render Settings")]
        [Description("The target render frequency.")]
        public double TargetRenderFrequency
        {
            get { return configuration.TargetRenderFrequency; }
            set { configuration.TargetRenderFrequency = value; }
        }

        [Category("Render Settings")]
        [Description("The optional target update frequency. If a value is not specified, it will be the same as the render frequency.")]
        public double? TargetUpdateFrequency
        {
            get { return configuration.TargetUpdateFrequency; }
            set { configuration.TargetUpdateFrequency = value; }
        }

        [Category("Render Settings")]
        [Description("Specifies the initial shader window render state.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return configuration.RenderState; }
        }

        [Category("Render Settings")]
        [Description("Specifies the graphics mode of the shader window.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public GraphicsModeConfiguration GraphicsMode
        {
            get { return configuration.GraphicsMode; }
            set { configuration.GraphicsMode = value; }
        }

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
