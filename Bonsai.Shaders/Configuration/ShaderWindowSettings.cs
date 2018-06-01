using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
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
        [Description("Specifies the initial shader window render state.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
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

        class SettingsConverter : ExpandableObjectConverter
        {
            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                var properties = base.GetProperties(context, value, attributes);
                return properties.Sort(new[] { "Title", "Width", "Height" });
            }
        }
    }
}
