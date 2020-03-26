using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [TypeConverter(typeof(SettingsConverter))]
    [XmlRoot(Namespace = Constants.XmlNamespace)]
    public class ShaderWindowSettings
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();
        readonly ShaderConfigurationCollection shaders = new ShaderConfigurationCollection();
        readonly TextureConfigurationCollection textures = new TextureConfigurationCollection();
        readonly MeshConfigurationCollection meshes = new MeshConfigurationCollection();

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

        [Category("Window Style")]
        [Description("The width of the shader window surface.")]
        public int Width { get; set; }

        [Category("Window Style")]
        [Description("The height of the shader window surface.")]
        public int Height { get; set; }

        [Category("Window Style")]
        [Description("The title of the shader window.")]
        public string Title { get; set; }

        [Category("Render Settings")]
        [Description("Specifies V-Sync configuration for the shader window.")]
        public VSyncMode VSync { get; set; }

        [Category("Render Settings")]
        [Description("Specifies whether to synchronize buffer swaps across application windows.")]
        public bool SwapSync { get; set; }

        [XmlIgnore]
        [Category("Render Settings")]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; }

        [Browsable(false)]
        [XmlElement("ClearColor")]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        [Category("Render Settings")]
        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask { get; set; }

        [Category("Window Style")]
        [Description("Specifies whether to hide or show the mouse cursor over the shader window.")]
        public bool CursorVisible { get; set; }

        [Category("Window Style")]
        [Description("The optional starting location of the shader window.")]
        public Point? Location { get; set; }

        [Category("Window Style")]
        [Description("The initial shader window border.")]
        public WindowBorder WindowBorder { get; set; }

        [Category("Window Style")]
        [Description("The initial shader window state.")]
        public WindowState WindowState { get; set; }

        [Category("Render Settings")]
        [Description("The display device index on which to create the shader window.")]
        public DisplayIndex DisplayDevice { get; set; }

        [Category("Render Settings")]
        [Description("The target render frequency.")]
        public double TargetRenderFrequency { get; set; }

        [Category("Render Settings")]
        [Description("The optional target update frequency. If a value is not specified, it will be the same as the render frequency.")]
        public double? TargetUpdateFrequency { get; set; }

        [Category("Render Settings")]
        [Description("Specifies the initial shader window render state.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [Category("Render Settings")]
        [Description("Specifies the graphics mode of the shader window.")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public GraphicsModeConfiguration GraphicsMode { get; set; }

        [Browsable(false)]
        public ShaderConfigurationCollection Shaders
        {
            get { return shaders; }
        }

        [Browsable(false)]
        public TextureConfigurationCollection Textures
        {
            get { return textures; }
        }

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
