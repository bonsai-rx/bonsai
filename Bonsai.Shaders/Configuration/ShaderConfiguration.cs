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
    public class ShaderConfiguration
    {
        readonly FramebufferConfiguration framebuffer = new FramebufferConfiguration();
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();
        readonly TextureBindingConfigurationCollection textureBindings = new TextureBindingConfigurationCollection();

        public ShaderConfiguration()
        {
            Enabled = true;
        }

        public string Name { get; set; }

        [Category("State")]
        public bool Enabled { get; set; }

        [Category("Shaders")]
        [Editor("Bonsai.Shaders.Design.VertScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string VertexShader { get; set; }

        [Category("Shaders")]
        [Editor("Bonsai.Shaders.Design.GeomScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string GeometryShader { get; set; }

        [Category("Shaders")]
        [Editor("Bonsai.Shaders.Design.FragScriptEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FragmentShader { get; set; }

        [Category("State")]
        [TypeConverter(typeof(MeshNameConverter))]
        public string MeshName { get; set; }

        [Category("State")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [Category("State")]
        [Editor("Bonsai.Shaders.Configuration.Design.TextureBindingConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public TextureBindingConfigurationCollection TextureBindings
        {
            get { return textureBindings; }
        }

        [Category("State")]
        [Editor("Bonsai.Shaders.Configuration.Design.FramebufferAttachmentConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebuffer.FramebufferAttachments; }
        }

        public Shader CreateShader(ShaderWindow window)
        {
            var vertexSource = File.ReadAllText(VertexShader);
            var geometrySource = !string.IsNullOrEmpty(GeometryShader) ? File.ReadAllText(GeometryShader) : null;
            var fragmentSource = File.ReadAllText(FragmentShader);

            var shader = new Shader(
                Name, window,
                vertexSource,
                geometrySource,
                fragmentSource,
                renderState,
                textureBindings,
                framebuffer);
            shader.Enabled = Enabled;
            return shader;
        }

        public override string ToString()
        {
            var name = Name;
            return string.IsNullOrEmpty(name) ? GetType().Name : name;
        }
    }
}
