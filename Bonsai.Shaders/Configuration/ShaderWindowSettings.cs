using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents the configuration settings used to initialize a shader window.
    /// </summary>
    [TypeConverter(typeof(SettingsConverter))]
    [XmlRoot(Namespace = Constants.XmlNamespace)]
    public class ShaderWindowSettings
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();
        readonly ShaderConfigurationCollection shaders = new ShaderConfigurationCollection();
        readonly TextureConfigurationCollection textures = new TextureConfigurationCollection();
        readonly MeshConfigurationCollection meshes = new MeshConfigurationCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderWindowSettings"/> class.
        /// </summary>
        public ShaderWindowSettings()
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
            SwapSync = true;
        }

        /// <summary>
        /// Gets or sets the width of the shader window, in pixels.
        /// </summary>
        [Category("Window Style")]
        [Description("The width of the shader window, in pixels.")]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the shader window, in pixels.
        /// </summary>
        [Category("Window Style")]
        [Description("The height of the shader window, in pixels.")]
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the title of the shader window.
        /// </summary>
        [Category("Window Style")]
        [Description("The title of the shader window.")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the V-Sync configuration for shader
        /// window buffer swaps.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the V-Sync configuration for shader window buffer swaps.")]
        public VSyncMode VSync { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether to synchronize buffer swaps
        /// across application windows.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies whether to synchronize buffer swaps across application windows.")]
        public bool SwapSync { get; set; }

        /// <summary>
        /// Gets or sets the color used to clear the framebuffer before rendering.
        /// </summary>
        [XmlIgnore]
        [Category("Render Settings")]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; }

        /// <summary>
        /// Gets or sets an HTML representation of the clear color value for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(ClearColor))]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        /// <summary>
        /// Gets or sets a value specifying which buffers to clear before rendering.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether to hide or show the mouse cursor
        /// over the shader window.
        /// </summary>
        [Category("Window Style")]
        [Description("Specifies whether to hide or show the mouse cursor over the shader window.")]
        public bool CursorVisible { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the starting location of the shader window.
        /// If no value is specified, the window will be located at the center of the screen.
        /// </summary>
        [Category("Window Style")]
        [Description("Specifies the optional starting location of the shader window.")]
        public Point? Location { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the shader window border.
        /// </summary>
        [Category("Window Style")]
        [Description("Specifies the shader window border.")]
        public WindowBorder WindowBorder { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the starting state of the shader window.
        /// </summary>
        [Category("Window Style")]
        [Description("Specifies the starting state of the shader window.")]
        public WindowState WindowState { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the display device index on which to
        /// create the shader window.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the display device index on which to create the shader window.")]
        public DisplayIndex DisplayDevice { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the target render frequency. A value of zero
        /// indicates the maximum possible frequency will be used to generate render events.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the target render frequency.")]
        public double TargetRenderFrequency { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the target update frequency. If no value is
        /// specified, the value of the target render frequency will be used.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the target update frequency. If no value is specified, the target render frequency will be used.")]
        public double? TargetUpdateFrequency { get; set; }

        /// <summary>
        /// Gets the collection of configuration objects specifying the initial render
        /// state of the shader window graphics context.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the initial render state of the shader window graphics context.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        /// <summary>
        /// Gets or sets a value specifying the graphics mode of the shader window.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the graphics mode of the shader window.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public GraphicsModeConfiguration GraphicsMode { get; set; }

        /// <summary>
        /// Gets the collection of shader resources to be loaded when creating
        /// the shader window.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public ShaderConfigurationCollection Shaders
        {
            get { return shaders; }
        }

        /// <summary>
        /// Gets the collection of texture resources to be loaded when creating
        /// the shader window.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public TextureConfigurationCollection Textures
        {
            get { return textures; }
        }

        /// <summary>
        /// Gets the collection of mesh resources to be loaded when creating the
        /// shader window.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public MeshConfigurationCollection Meshes
        {
            get { return meshes; }
        }
    }

    class SettingsConverter : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var properties = new PropertyCollection(base
                .GetProperties(context, value, attributes)
                .Sort(new[] { "Title", "Width", "Height" }));
            var swapSync = properties["SwapSync"];
            properties.Remove(swapSync);
            properties.Add(swapSync);
            return properties;
        }

        class PropertyCollection : PropertyDescriptorCollection
        {
            public PropertyCollection(PropertyDescriptorCollection properties)
                : base(ToArray(properties), false)
            {
            }

            static PropertyDescriptor[] ToArray(PropertyDescriptorCollection properties)
            {
                var result = new PropertyDescriptor[properties.Count];
                properties.CopyTo(result, 0);
                return result;
            }

            public override PropertyDescriptorCollection Sort()
            {
                return this;
            }

            public override PropertyDescriptorCollection Sort(IComparer comparer)
            {
                return this;
            }

            public override PropertyDescriptorCollection Sort(string[] names)
            {
                return this;
            }

            public override PropertyDescriptorCollection Sort(string[] names, IComparer comparer)
            {
                return this;
            }
        }
    }
}
