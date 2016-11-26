using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    public class MaterialConfiguration
    {
        readonly FramebufferConfiguration framebuffer = new FramebufferConfiguration();
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();
        readonly UniformConfigurationCollection shaderUniforms = new UniformConfigurationCollection();
        readonly TextureBindingConfigurationCollection textureBindings = new TextureBindingConfigurationCollection();

        public MaterialConfiguration()
        {
            Enabled = true;
        }

        [Description("The name of the material.")]
        public string Name { get; set; }

        [Category("State")]
        [Description("Specifies whether the material is active.")]
        public bool Enabled { get; set; }

        [Category("Shaders")]
        [Description("Specifies the path to the vertex shader program.")]
        [Editor("Bonsai.Shaders.Design.VertScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string VertexShader { get; set; }

        [Category("Shaders")]
        [Description("Specifies the path to the geometry shader program.")]
        [Editor("Bonsai.Shaders.Design.GeomScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string GeometryShader { get; set; }

        [Category("Shaders")]
        [Description("Specifies the path to the fragment shader program.")]
        [Editor("Bonsai.Shaders.Design.FragScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FragmentShader { get; set; }

        [Category("State")]
        [Description("The name of the mesh geometry to draw.")]
        [TypeConverter(typeof(MeshNameConverter))]
        public string MeshName { get; set; }

        [Category("State")]
        [Description("Specifies any render states that are required to draw the material.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [Category("State")]
        [Description("Specifies any shader uniform values that are required to draw the material.")]
        [Editor("Bonsai.Shaders.Configuration.Design.UniformConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public UniformConfigurationCollection ShaderUniforms
        {
            get { return shaderUniforms; }
        }

        [Category("State")]
        [Description("Specifies any texture bindings that are required to draw the material.")]
        [Editor("Bonsai.Shaders.Configuration.Design.TextureBindingConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public TextureBindingConfigurationCollection TextureBindings
        {
            get { return textureBindings; }
        }

        [Category("State")]
        [Description("Specifies any framebuffer attachments that are required to draw the material.")]
        [Editor("Bonsai.Shaders.Configuration.Design.FramebufferAttachmentConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebuffer.FramebufferAttachments; }
        }

        public Material CreateMaterial(ShaderWindow window)
        {
            var vertexSource = File.ReadAllText(VertexShader);
            var geometrySource = !string.IsNullOrEmpty(GeometryShader) ? File.ReadAllText(GeometryShader) : null;
            var fragmentSource = File.ReadAllText(FragmentShader);

            var material = new Material(
                Name, window,
                vertexSource,
                geometrySource,
                fragmentSource,
                renderState,
                shaderUniforms,
                textureBindings,
                framebuffer);
            material.Enabled = Enabled;
            return material;
        }

        public override string ToString()
        {
            var name = Name;
            return string.IsNullOrEmpty(name) ? GetType().Name : name;
        }
    }
}
